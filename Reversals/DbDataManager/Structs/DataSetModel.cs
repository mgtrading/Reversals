namespace Reversals.DbDataManager.Structs
{
    public struct DataSetModel
    {
        public int Id;
        public string DataSetName;
        public int SymbolId;
        public double TimeValue;
        public double Commission;
        public int Multiplier;
        public int ContractSize;
        public double PointValue;
        public double Zim;
        public double TickSize;

        public double StopLevelDef;
        public double StopLevelMin;
        public double StopLevelMax;
        public double StopLevelStep;
        public double ReversalLevelDef;
        public double ReversalLevelMin;
        public double ReversalLevelMax;
        public double ReversalLevelStep;
    }
}
