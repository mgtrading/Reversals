using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Reversals.DbDataManager.Structs;

namespace Reversals.DataContainer
{
    public class Data
    {
        #region ENUMS
        public enum DataFileType
        {
            TextFromTs,
            TickFromDb
        }
        #endregion

        #region VARIABLES
        private readonly List<Tick> _bars = new List<Tick>();
        private readonly DataFileType _fileType;
        private readonly string _source;
        private readonly List<TickDataModel> _contractList;
        private readonly int _timeZoneOffset;
        #endregion

        #region CONSTRUCTORS

        public Data(string source, DataFileType fileType)
        {
            _source = source;
            _fileType = fileType;
        }

        public Data(List<Tick> bars)
        {
            _bars = bars;
        }

        public Data(List<TickDataModel> contractList, DataFileType fileType)
        {
            _contractList = contractList;
            _fileType = fileType;
            _timeZoneOffset = 0;
        }
        public Data(List<TickDataModel> contractList, DataFileType fileType,int timeZoneOffset)
        {
            _contractList = contractList;
            _fileType = fileType;
            _timeZoneOffset = timeZoneOffset;
        }

        public Data()
        {
            // TODO: Complete member initialization
        }
        #endregion

        #region CREATING DATA METHODS
        public void CreateData()
        {
            if (_fileType == DataFileType.TextFromTs)
            {

                //CultureInfo provider = CultureInfo.InvariantCulture;
                String[] values = File.ReadAllLines(_source);

                if (values.Length <= 1)
                    return;

                for (int i = 0; i < values.Length; i++)
                {
                    String[] bar = values[i].Split('	');
                    string dateStr = bar[0];
                    DateTime dt = dateStr.IndexOf("/", StringComparison.Ordinal) != -1 ? DateTime.ParseExact(dateStr, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) :
                                      DateTime.ParseExact(dateStr, "dd.MM.yyyy HH:mm:ss", null);

                    int intradayIndex = dt.Hour * 10000 + dt.Minute * 100 + dt.Second;
                    int addIndex ;
                    if (i == 0)
                        addIndex = 0;
                    else
                    {
                        if (intradayIndex == _bars[i - 1].IntradayIndex)
                            addIndex = _bars[i - 1].AddIntradayIndex + 1;
                        else
                            addIndex = 0;
                    }

                    _bars.Add(new Tick(dt,
                                     Double.Parse(bar[1], NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture),
                                     intradayIndex, 
                                     addIndex, 
                                     -1));
                }
            }
            else if (_fileType == DataFileType.TickFromDb)
            {
                int i = 0;

                var tempList = _contractList.OrderBy(pos => pos.Date).ToList();
               
                foreach (var item in tempList)
                {
                    DateTime dt = item.Date.AddHours(_timeZoneOffset);


                    int intradayIndex = dt.Hour * 10000 + dt.Minute * 100 + dt.Second;
                    int addIndex ;
                    if (i == 0)
                        addIndex = 0;
                    else
                    {
                        if (intradayIndex == _bars[i - 1].IntradayIndex)
                            addIndex = _bars[i - 1].AddIntradayIndex + 1;
                        else
                            addIndex = 0;
                    }

                    _bars.Add(new Tick(dt, item.Price, intradayIndex, addIndex, -1));

                    i++;

                }
            }
        }
        #endregion

        #region PROPERTIES
        public int Count
        {
            get { return _bars.Count; }
        }

        public List<Tick> Bars
        {
            get { return _bars; }
        }

        public Data BarsRange(DateTime start, DateTime end, int barsBeforeStart)
        {
            Tick startTick = _bars.Find(bar => bar.Time >= start);

            if (startTick == null)
                return new Data();
            Tick endTick = _bars.FindLast(bar => bar.Time <= end);

            int startIndex = _bars.IndexOf(startTick) - barsBeforeStart >= 0 ? _bars.IndexOf(startTick) - barsBeforeStart : 0;
            int endIndex = _bars.IndexOf(endTick);

            return new Data(_bars.GetRange(startIndex, endIndex - startIndex + 1));
        }

        public Tick this[int i]
        {
            get { return _bars[i]; }
        }

        public List<DateTime> Times
        {
            get
            {
                return _bars.Select(bar => bar.Time).ToList();
            }
        }

        public List<double> Closes
        {
            get
            {
                return _bars.Select(bar => bar.Close).ToList();
            }
        }
        #endregion
    }
}