        
using System;
using System.Collections.Generic;
using System.Threading;
using Reversals.DbDataManager;
using Bloomberglp.Blpapi;
using System.Collections;

namespace Reversals.CollectingPriceData
{
    class IntradayTick
    {
        private static int ContractColected
        {
            get { return _collected; }
        }
        private static int _collected;
        private static List<string> _contracts;
        private static Semaphore _waitEndOfCollection;

        public delegate void ProgressEventHandler(int value);       
        public static event ProgressEventHandler ProgressEvent;

        public delegate void ErrorEventHandler(int errorCode);
        public static event ErrorEventHandler ErrorEvent;      

        public static void CollectContracts(List<string> contracts, DateTime start, DateTime end)
        {
            _collected = 0;
            _waitEndOfCollection = new Semaphore(0, 1);

            _contracts = contracts;
            NotifyProgress();

            new Thread(() =>
            {
                foreach (var item in _contracts)
                {
                    new IntradayTick(item, start, end).Run();
                    _waitEndOfCollection.WaitOne();
                    _collected++;
                    NotifyProgress();
                }

            }).Start();
        }

        public static bool IsBusy()
        {
            if (_contracts != null)
                return ContractColected != _contracts.Count;
            return false;
        }

        private static void NotifyProgress(double percent = 1)
        {
            if (ProgressEvent != null)
            {
                var resPercent = (int)Math.Round((ContractColected - 1 + percent) * 100 / _contracts.Count);
                ProgressEvent(resPercent);
            }
        }

        private static void NotifyError(int error)
        {
            if (ErrorEvent !=  null)
            {
                ErrorEvent(error);
            }
        }

        private static readonly Name TickData = new Name("tickData");
        //private static readonly Name CondCode = new Name("conditionCodes");
        //private static readonly Name Size = new Name("size");
        private static readonly Name Time = new Name("time");
        //private static readonly Name Type = new Name("type");
        private static readonly Name Value = new Name("value");
        private static readonly Name ResponseError = new Name("responseError");
        private static readonly Name Category = new Name("category");
        private static readonly Name Message = new Name("message");

        private readonly string _dHost;
        private readonly int _dPort;
        private readonly string _dSecurity;
        private readonly Datetime _dStartDateTime;
        private readonly Datetime _dEndDateTime;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly ArrayList _dEvents;
        private readonly bool _dConditionCodes;
        private static volatile bool _shouldStop;    

        private IntradayTick(string contract, DateTime dateTimeStart, DateTime dateTimeEnd)
        {
            _dSecurity = contract;
            _startDate = dateTimeStart;
            _endDate = dateTimeEnd;
            _dStartDateTime = new Datetime(dateTimeStart.ToUniversalTime());
            _dEndDateTime = new Datetime(dateTimeEnd.ToUniversalTime());

            _dHost = "localhost";
            _dPort = 8194;
            _dEvents = new ArrayList {"TRADE"};
            _dConditionCodes = false;
            _shouldStop = false;
        }

        private void Run()
        {

            var sessionOptions = new SessionOptions { ServerHost = _dHost, ServerPort = _dPort };

            Console.WriteLine(@"Connecting to " + _dHost + @":" + _dPort);
            var session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                Console.Error.WriteLine("Failed to start session.");
                _waitEndOfCollection.Release();
                return;
            }
            if (!session.OpenService("//blp/refdata"))
            {
                Console.Error.WriteLine("Failed to open //blp/refdata");
                _waitEndOfCollection.Release();
                return;
            }

            DataManager.CreateTableForContract(_dSecurity);

            DataManager.DeleteTicks(_dSecurity, _startDate, _endDate);

            SendIntradayTickRequest(session);

            EventLoop(session);

            session.Stop();
        }

        private void SendIntradayTickRequest(Session session)
        {
            Service refDataService = session.GetService("//blp/refdata");
            Request request = refDataService.CreateRequest("IntradayTickRequest");

            request.Set("security", _dSecurity);

            // Add fields to request
            var eventTypes = request.GetElement("eventTypes");
            foreach (object t in _dEvents)
            {
                eventTypes.AppendValue((string)t);
            }

            // All times are in GMT
            request.Set("startDateTime", _dStartDateTime);
            request.Set("endDateTime", _dEndDateTime);

            if (_dConditionCodes)
            {
                request.Set("includeConditionCodes", true);
            }

            Console.WriteLine(@"Sending Request: " + request);
            session.SendRequest(request, null);
        }

        private void EventLoop(Session session)
        {
            bool done = false;
            while (!done)
            {

                if (_shouldStop)
                {
                    break;
                }
                Event eventObj = session.NextEvent();
                if (eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    Console.WriteLine(@"Processing Partial Response");
                    ProcessResponseEvent(eventObj);
                }
                else if (eventObj.Type == Event.EventType.RESPONSE)
                {
                    Console.WriteLine(@"Processing Response");
                    ProcessResponseEvent(eventObj);
                    done = true;
                }
                else
                {
                    foreach (var msg in eventObj)
                    {
                        Console.WriteLine(msg.AsElement);

                        if (eventObj.Type == Event.EventType.SESSION_STATUS)
                        {                            
                            if (msg.MessageType.Equals("SessionTerminated"))
                            {
                                done = true;
                            }
                        }
                    }
                }
            }
            _waitEndOfCollection.Release();
        }



        private void ProcessResponseEvent(IEnumerable<Message> eventObj)
        {
            foreach (var msg in eventObj)
            {
                if (msg.HasElement(ResponseError))
                {
                    if (msg.GetElement(ResponseError).GetElementAsString(Category) == "BAD_SEC")
                    {
                        NotifyError(2);
                    }
                    printErrorInfo("REQUEST FAILED: ", msg.GetElement(ResponseError));
                    continue;
                }
                ProcessMessage(msg);
            }
        }

        private void printErrorInfo(string leadingStr, Element errorInfo)
        {
            Console.WriteLine(@"{0}{1} ({2})", leadingStr, errorInfo.GetElementAsString(Category), errorInfo.GetElementAsString(Message));
        }

        private void ProcessMessage(Message msg)
        {
            Element data = msg.GetElement(TickData).GetElement(TickData);
            int numItems = data.NumValues;


            for (int i = 0; i < numItems; ++i)
            {
                Element item = data.GetValueAsElement(i);
                Datetime time = item.GetElementAsDate(Time);
                
                var value = item.GetElementAsFloat64(Value);
           


                var sysDatetime =
                    new DateTime(time.Year, time.Month, time.DayOfMonth,
                            time.Hour, time.Minute, time.Second, time.MilliSecond);

                double prgrs = (_endDate - _startDate).TotalDays / (sysDatetime - _startDate).TotalDays;

                NotifyProgress(prgrs);
                //TODO Notify prgress
                
                if (_shouldStop)
                {
                    break;
                }

                DataManager.AddTick(_dSecurity, sysDatetime, value);
            }
            DataManager.CommitQueue();
        }

        public static void Stop()
        {
            _shouldStop = true;
        }
    }

/*
    class TestFormCotrolHelper
    {
        delegate void UniversalVoidDelegate();

        /// <summary>
        /// Call form controll action from different thread
        /// </summary>
        public static void ControlInvoke(DevComponents.DotNetBar.ProgressBarItem control, Action function)
        {
            try
            {
                if (control.IsDisposed)
                    return;

                if (control.InvokeRequired)
                {
                    control.Invoke(new UniversalVoidDelegate(() => ControlInvoke(control, function)));
                    return;
                }
                function();
            }
            catch (Exception)
            {
            }
        }
    }
*/
}
