using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data;

namespace Reversals
{
    public class TemporaryDatasets
    {
        private double _tv;
        private double _commission;
        private double _multiplier;
        private double _contractSize;
        private double _pointValue;
        private double _zim;
        private double _tickSize;
        private readonly int _defaultStopLevel;

        private Dictionary<string,double> _aliParamList;
        private Dictionary<string, double> _goldParamList;
        private Dictionary<string, double> _wtiParamList;
        private Dictionary<string, double> _copperParamList;

        private readonly DataTable _parameterProvider;
        private readonly DataTable _optimalizableparamsProvider;
       



 public TemporaryDatasets()
    {
        _parameterProvider = new DataTable();
        _optimalizableparamsProvider = new DataTable();
        var column = new DataColumn("Variables");
        var column2 = new DataColumn("Default Value");
        _parameterProvider.Columns.Add(column);
        _parameterProvider.Columns.Add(column2);
        _parameterProvider.Columns["Variables"].ReadOnly = true;

       column = new DataColumn("Triggers");
       _optimalizableparamsProvider.Columns.Add(column);
       column = new DataColumn("Default Value");
       _optimalizableparamsProvider.Columns.Add(column);
       column = new DataColumn("MinValue");
         _optimalizableparamsProvider.Columns.Add(column);
        column = new DataColumn("MaxValue");
        _optimalizableparamsProvider.Columns.Add(column);
        column = new DataColumn("Step");
        _optimalizableparamsProvider.Columns.Add(column);

        _optimalizableparamsProvider.Columns["Triggers"].ReadOnly = true;
        _defaultStopLevel = 80;
    }

    public void InitAliDataSet(double tv, double commission, double mult, double contSize, double pointvalue, double zim, double ticksize)
    {
     _parameterProvider.Clear();
     _tv = tv;
     _commission =commission;
     _multiplier = mult;
     _contractSize = contSize;
     _pointValue = pointvalue;
     _tickSize = ticksize;
     _zim = _pointValue * _defaultStopLevel;

     _aliParamList = new Dictionary<string,double>
         {
              {"Time Value",_tv} ,
              {"Commission", _commission},
              {"Multiplier",_multiplier},
              {"Contract Size",_contractSize},
              {"Point Value",_pointValue},
              {"ZIM",_zim}  ,
              {"Tick Size",_tickSize}
         };

     DataRow nameDataSet = _parameterProvider.NewRow();
     nameDataSet[0] = "DataSet Name";
     nameDataSet[1] = "DataSet for File";
     _parameterProvider.Rows.Add(nameDataSet);

     foreach (KeyValuePair<string,double> keys in _aliParamList.ToArray())
     {
         DataRow row = _parameterProvider.NewRow();
         row[0] = keys.Key ;
         row[1] = keys.Value;
         _parameterProvider.Rows.Add(row);
         
     }
 }
    public void InitGoldDataSet(double tv, double commission, double mult, double contSize, double pointvalue, double zim, double ticksize)
    {
        _parameterProvider.Clear();
        _tv = tv;
        _commission = commission;
        _multiplier = mult;
        _contractSize = contSize;
        _pointValue = pointvalue;
        _tickSize = ticksize;
        _zim = _pointValue * _defaultStopLevel;

        _goldParamList = new Dictionary<string,double>
         {
              {"Time Value",_tv} ,
              {"Commission", _commission},
              {"Multiplier",_multiplier},
              {"Contract Size",_contractSize},
              {"Point Value",_pointValue},
              {"ZIM",_zim}  ,
              {"Tick Size",_tickSize}
         };

     foreach (KeyValuePair<string,double> keys in _goldParamList.ToArray())
     {
         DataRow row = _parameterProvider.NewRow();
         row[0] = keys.Key ;
         row[1] = keys.Value;
         _parameterProvider.Rows.Add(row);
         
     }
    }
    public void InitWtiDataSet(double tv, double commission, double mult, double contSize, double pointvalue, double zim, double ticksize)
    {
        _parameterProvider.Clear();
        _tv = tv;
        _commission = commission;
        _multiplier = mult;
        _contractSize = contSize;
        _pointValue = pointvalue;
        _tickSize = ticksize;
        _zim = _pointValue * _defaultStopLevel;

        _wtiParamList =  new Dictionary<string,double>
         {
              {"Time Value",_tv} ,
              {"Commission", _commission},
              {"Multiplier",_multiplier},
              {"Contract Size",_contractSize},
              {"Point Value",_pointValue},
              {"ZIM",_zim}  ,
              {"Tick Size",_tickSize}
         };

     foreach (KeyValuePair<string,double> keys in _wtiParamList.ToArray())
     {
         DataRow row = _parameterProvider.NewRow();
         row[0] = keys.Key ;
         row[1] = keys.Value;
         _parameterProvider.Rows.Add(row);
         
     }
    }
    public void InitCopperDataSet(double tv, double commission, double mult, double contSize, double pointvalue, double zim, double ticksize)
    {
        _parameterProvider.Clear();
        _tv = tv;
        _commission = commission;
        _multiplier = mult;
        _contractSize = contSize;
        _pointValue = pointvalue;
        _tickSize = ticksize;
        _zim = _pointValue * _defaultStopLevel;
       
      

        _copperParamList = new Dictionary<string,double>
         {
              {"Time Value",_tv} ,
              {"Commission", _commission},
              {"Multiplier",_multiplier},
              {"Contract Size",_contractSize},
              {"Point Value",_pointValue},
              {"ZIM",_zim}  ,
              {"Tick Size",_tickSize}
         };

     foreach (KeyValuePair<string,double> keys in _copperParamList.ToArray())
     {
         DataRow row = _parameterProvider.NewRow();
         row[0] = keys.Key ;
         row[1] = keys.Value;
         _parameterProvider.Rows.Add(row);
         
     }
    }
    public void InitOptimParams()
    {
        DataRow row = _optimalizableparamsProvider.NewRow();
        var list1 = new ArrayList
       {
           "Stop Level",
           80 * _tickSize,
           60 * _tickSize,
           100 * _tickSize,
           10 * _tickSize
       };
        row.ItemArray = list1.ToArray();
        _optimalizableparamsProvider.Rows.Add(row);

        row = _optimalizableparamsProvider.NewRow();
        list1 = new ArrayList
       {
           "Reversal Level",
           80 * _tickSize,
           60 * _tickSize,
           100 * _tickSize,
           10 * _tickSize
       };
        row.ItemArray = list1.ToArray();
        _optimalizableparamsProvider.Rows.Add(row);
    }

    public DataTable OptimParams
    {
        get
        {
            InitOptimParams();
            return _optimalizableparamsProvider;
        }
    }
    public DataTable AliDataSet252519
    {
        
        get
        {
            
            InitAliDataSet(3043.00, 0.85, 1,25,-0.702,-17.55,1);
                 return _parameterProvider;
           
        }

    }
    public DataTable AliDataSet151519
    {

        get
        {
           
            InitAliDataSet(3043.00, 0.85, 1, 25, -0.702, -10.53, 1);
            return _parameterProvider;

        }

    }

        public double AliPv
        {
            get { return -0.702; }
        }

        public double AliZim
        {
            get { return -10.53; }
        }

        public double AliTickSize
        {
            get { return 1; }
        }

    public DataTable CopperDataSet505018
    {
       get{
            const double tv = 3083.00;
            const double commission = 0.85;
            const int multiplier = 1;
            const int contractSize = 25;
         
            const double pointValue = -0.055;
            const double tickSize = 1.0;
            var zim = pointValue * _defaultStopLevel;
         

            InitCopperDataSet(Math.Round(tv,2), commission, multiplier, contractSize, pointValue, zim, tickSize);
            return _parameterProvider;
          }
    }
    public DataTable CopperDataSet505015
    {
        get
        {
            const double tv = 3000.00;
            const double commission = 0.85;
            const int multiplier = 1;
            const int contractSize = 25;
            const double pointValue = -0.0572;
            const double tickSize = 1.0;
            var zim = pointValue * _defaultStopLevel;
           
            InitCopperDataSet(tv, commission, multiplier, contractSize, pointValue, zim, tickSize);
            return _parameterProvider;
        }
    }
    public DataTable WtiDataSet808021Real
    {
        get
        {
           const double tv = 3035.00;
           const double commission = 0.009;
           const int multiplier = 100;
           const int contractSize = 1000;
           const double pointValue = -0.058712;
           const double tickSize = 0.01;
           var zim = pointValue * _defaultStopLevel;
           
            InitWtiDataSet(tv,commission,multiplier,contractSize,pointValue,zim,tickSize);
            return _parameterProvider;
        }

    }
    public DataTable WtiDataSet808021
    {
        get
        {
            const double tv = 3053.00;
            const double commission = 0.009;
            const int multiplier = 100;
            const int contractSize = 1000;
            const double pointValue = -0.064156;
            const double tickSize = 0.01;
            var zim = pointValue * _defaultStopLevel;
           
            InitWtiDataSet(tv, commission, multiplier, contractSize, pointValue, zim, tickSize);
            return _parameterProvider;
        }

    }
    public DataTable GoldDataSet101021Real
    {
        get
        {
            const double tv = 3020.00;
            const double commission = 0.07;
            const int multiplier = 1;
            const int contractSize = 100;
            const double pointValue = -0.2392;
            const double tickSize = 1.0;
            var zim = pointValue * _defaultStopLevel;
            
            InitGoldDataSet(tv,commission,multiplier,contractSize,pointValue,zim,tickSize);
            return _parameterProvider;
        }

    }
    public DataTable GoldDataSet101025
    {
        get
        {
            const double tv = 3020.00;
            const double commission = 0.07;
            const int multiplier = 1;
            const int contractSize = 100;
            const double pointValue = -0.184;
            const double tickSize = 1.0;
            var zim = pointValue * _defaultStopLevel;
            
            InitGoldDataSet(tv, commission, multiplier, contractSize, pointValue, zim, tickSize);
            return _parameterProvider;
        }

    }
    }
}
