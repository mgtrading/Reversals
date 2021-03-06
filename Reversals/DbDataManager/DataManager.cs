﻿using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data;
using System.Globalization;
using Reversals.DbDataManager.Structs;

namespace Reversals.DbDataManager
{
    static class DataManager
    {
        #region VARIABLES

        private static MySqlConnection _connection;
        private static MySqlCommand _sqlCommand;
        private const string TblContracts = "tbl_contracts";
        private const string TblDatasets = "tbl_datasets";
        private const string TblResults = "tbl_calendar_results";
        private const string TblSummaryResults = "tbl_summary_results";
        private static readonly List<string> QueryQueue = new List<string>();
        private const string TblWeeklyDataTradeResults = "tbl_weekly_tradeResults";
        private const string TblWeeklyDataOrderResults = "tbl_weekly_orderResults";
        #endregion


        #region CONTRACT

        /// <summary>
        /// Add new contract to db with creating table for it
        /// </summary>
        /// <param name="contract"></param>
        public static void AddContract(string contract)//, DateTime startDate, DateTime endDate)
        {
            var sql = "INSERT IGNORE INTO " + TblContracts
            + " (`ContractName`)"
            + "VALUES('" + contract + "');COMMIT;";

            if (DoSql(sql))
                CreateTableForContract(contract);
        }

        /// <summary>
        /// Edit contract
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public static void EditContract(string oldName, string newName)
        {
            var sql = "UPDATE `" + TblContracts + "` SET `ContractName`='"+newName+"' WHERE `ContractName`='" + oldName + "';COMMIT;";

            DoSql(sql);

            sql = "RENAME TABLE " + GetTableNameFromContract(oldName) + " TO "+GetTableNameFromContract(newName)+";COMMIT;";

            DoSql(sql);
        }

        /// <summary>
        /// Delete contract
        /// </summary>
        /// <param name="contract"></param>
        public static void DelContract(string contract)
        {
            var sql = "DELETE FROM `" + TblContracts + "` WHERE `ContractName`='" + contract + "';COMMIT;";

            DoSql(sql);

            sql = "DROP TABLE " + GetTableNameFromContract(contract) + ";COMMIT;";

            DoSql(sql);
        }


        /// <summary>
        /// With this function you can get all conracts
        /// names from DB
        /// </summary>
        /// <returns>List with all names and ids of contracts</returns>
        public static List<ContractModel> GetContracts()
        {
            var res = new List<ContractModel>();

            const string sql = "SELECT * FROM " + TblContracts;
            MySqlDataReader reader = GetReader(sql);
            if (reader != null)
            {
                while (reader.Read())
                {
                    var cm = new ContractModel { CountractId = reader.GetInt32(0), ContractName = reader.GetString(1) };
                    res.Add(cm);
                }

                reader.Close();
            }
            return res;
        }

        /// <summary>
        /// With this function you can get all data 
        /// of contract
        /// </summary>
        /// <param name="contractName"></param>
        /// <returns></returns>
        public static List<TickDataModel> GetContractData(string contractName)
        {
            var contractModelList = new List<TickDataModel>();

            var sql = "SELECT * FROM " + GetTableNameFromContract(contractName);
            var reader = GetReader(sql);
            if (reader != null)
            {
                while (reader.Read())
                {
                    var cm = new TickDataModel { Date = reader.GetDateTime(1), Price = reader.GetDouble(2) };

                    contractModelList.Add(cm);
                }

                reader.Close();
            }

            return contractModelList;
        }


        #endregion


        #region DATASET

        /// <summary>
        /// With this function you can get all datasets
        /// names from DB
        /// </summary>
        /// <returns></returns>
        public static List<DataSetModel> GetDatasets()
        {
            var res = new List<DataSetModel>();

            const string sql = "SELECT * FROM " + TblDatasets;
            var reader = GetReader(sql);
            if (reader != null)
            {
                while (reader.Read())
                {
                    var d = new DataSetModel
                    {
                        Id = reader.GetInt32(0),
                        DataSetName = reader.GetString(2),
                        SymbolId = reader.GetInt32(1),
                        TimeValue = reader.GetDouble(3),
                        Commission = reader.GetDouble(4),

                        Multiplier = reader.GetInt32(5),
                        ContractSize = reader.GetInt32(6),

                        PointValue = reader.GetDouble(7),
                        Zim = reader.GetDouble(8),
                        TickSize = reader.GetDouble(9),
                    };

                    res.Add(d);
                }

                reader.Close();
            }
            return res;
        }

        /// <summary>
        /// With this function you can get all data
        /// of dataset from DB
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        public static DataSetModel GetDatasetData(int datasetId)
        {
            var dsm = new DataSetModel();

            var sql = "SELECT * FROM " + TblDatasets + " WHERE `ID`= " + datasetId;
            var reader = GetReader(sql);
            if (reader != null)
            {
                if (reader.Read())
                {
                    dsm.Id = reader.GetInt32(0);
                    dsm.SymbolId = reader.GetInt32(1);
                    dsm.DataSetName = reader.GetString(2);
                    dsm.TimeValue = reader.GetDouble(3);
                    dsm.Commission = reader.GetDouble(4);

                    dsm.Multiplier = reader.GetInt32(5);
                    dsm.ContractSize = reader.GetInt32(6);

                    dsm.PointValue = reader.GetDouble(7);
                    dsm.Zim = reader.GetDouble(8);
                    dsm.TickSize = reader.GetDouble(9);

                    dsm.StopLevelDef = reader.GetDouble(10);
                    dsm.StopLevelMin = reader.GetDouble(11);
                    dsm.StopLevelMax = reader.GetDouble(12);
                    dsm.StopLevelStep = reader.GetDouble(13);

                    dsm.ReversalLevelDef = reader.GetDouble(14);
                    dsm.ReversalLevelMin = reader.GetDouble(15);
                    dsm.ReversalLevelMax = reader.GetDouble(16);
                    dsm.ReversalLevelStep = reader.GetDouble(17);

                }

                reader.Close();
            }

            return dsm;
        }


        /// <summary>
        /// Add new dataset to table
        /// </summary>
        /// <param name="dsm"></param>
        /// <returns>TRUE if adding success else return FALSE</returns>
        public static bool AddDataset(DataSetModel dsm)
        {
            String query = "INSERT IGNORE INTO " + TblDatasets;
            query += "(DataSetName, Symbol_ID,TimeValue, Commission, Multiplier, ContractSize, PointValue, ZIM, TickSize,"
            +" StopLevelDef, StopLevelMin, StopLevelMax, StopLevelStep,"
            + "  ReversalLevelDef, ReversalLevelMin, ReversalLevelMax, ReversalLevelStep) VALUES";
            query += "('";
            query += dsm.DataSetName + "',";
            query += dsm.SymbolId + ",";
            query += dsm.TimeValue + ",";
            query += dsm.Commission + ",";
            query += dsm.Multiplier + ",";
            query += dsm.ContractSize + ",";
            query += dsm.PointValue + ",";
            query += dsm.Zim + ",";
            query += dsm.TickSize + ",";

            query += dsm.StopLevelDef + ",";
            query += dsm.StopLevelMin + ",";
            query += dsm.StopLevelMax + ",";
            query += dsm.StopLevelStep + ",";

            query += dsm.ReversalLevelDef + ",";
            query += dsm.ReversalLevelMin + ",";
            query += dsm.ReversalLevelMax + ",";
            query += dsm.ReversalLevelStep + ");COMMIT;";

            return DoSql(query);
        }

        /// <summary>
        /// Edit dataset by id
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="dsm"></param>
        /// <returns>return TRUE if changes saved success</returns>
        public static bool EditDataset(int datasetId, DataSetModel dsm)
        {
            String query = "UPDATE " + TblDatasets + " SET "
                + " DataSetName = '" + dsm.DataSetName + "', "
                + " TimeValue = " + dsm.TimeValue + ", "
                + " Symbol_ID = " + dsm.SymbolId + ", "
                + " Commission = " + dsm.Commission + ","
                + " Multiplier = " + dsm.Multiplier + ","
                + " ContractSize = " + dsm.ContractSize + ","
                + " PointValue = " + dsm.PointValue + ","
                + " ZIM = " + dsm.Zim + ","
                + " TickSize = " + dsm.TickSize + ","

                + " StopLevelDef = " + dsm.StopLevelDef + ","
                + " StopLevelMin = " + dsm.StopLevelMin + ","
                + " StopLevelMax = " + dsm.StopLevelMax + ","
                + " StopLevelStep = " + dsm.StopLevelStep + ","

                + " ReversalLevelDef = " + dsm.ReversalLevelDef + ","
                + " ReversalLevelMin = " + dsm.ReversalLevelMin + ","
                + " ReversalLevelMax = " + dsm.ReversalLevelMax + ","
                + " ReversalLevelStep = " + dsm.ReversalLevelStep + "";
            query += " WHERE  ID = '" + datasetId + "'; COMMIT;";

            return DoSql(query);
        }

        /// <summary>
        /// delete dataset be his id
        /// </summary>
        /// <param name="datasetId"></param>
        public static void DelDataset(int datasetId)
        {
            DoSql("DELETE FROM `" + TblDatasets + "` WHERE ID = " + datasetId + " ;COMMIT;");
        }

        #endregion


        #region RESULTS

        /// <summary>
        /// Add new results to table with deleting old data
        /// with current symbolId and datasetId
        /// </summary>
        /// <param name="symbolId"></param>
        /// <param name="datasetId"></param>
        /// <param name="resultModel"></param>
        public static void AddResult(int symbolId, int datasetId, IEnumerable<ResultModel> resultModel)
        {
            // DELETE old data

            DelResult(symbolId, datasetId);

            // ADD new data
            foreach (var item in resultModel)
            {
                var strDate = Convert.ToDateTime(item.Date).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

                var query = "INSERT IGNORE INTO " + TblResults;
                query += "(symbol_id, dataset_id, Date, Pnl) VALUES";
                query += "(";
                query += symbolId + ",";
                query += datasetId + ",";
                query += "'" + strDate + "',";
                query += item.Pnl + ");COMMIT;";

                DoSql(query);
            }
        }

        /// <summary>
        /// Get results
        /// </summary>
        /// <param name="symbolId"></param>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        public static List<ResultModel> GetResult(int symbolId, int datasetId)
        {
            var result = new List<ResultModel>();

            var sql = "SELECT * FROM " + TblResults;
            sql += " WHERE Symbol_ID =" + symbolId + " AND Dataset_ID = " + datasetId + " ORDER BY `Date`";


            var reader = GetReader(sql);
            if (reader != null)
            {
                while (reader.Read())
                {
                    var rm = new ResultModel { Date = reader.GetDateTime(3), Pnl = reader.GetDouble(4) };

                    result.Add(rm);
                }

                reader.Close();
            }

            return result;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="symbolId"></param>
        /// <param name="datasetId"></param>
        public static void DelResult(int symbolId, int datasetId)
        {
            var query1 = "DELETE FROM " + TblResults;
            query1 += " WHERE Symbol_ID =" + symbolId + " AND Dataset_ID = " + datasetId + " ;COMMIT;";

            DoSql(query1);
        }

        #endregion


        #region SUMMARY RESULTS

        /// <summary>
        /// Add new results to table with deleting old data
        /// with current symbolId and datasetId
        /// </summary>
        /// <param name="symbolId"></param>
        /// <param name="datasetId"></param>
        /// <param name="summaryResultModel"></param>
        public static void AddSummaryResult(int symbolId, int datasetId, IEnumerable<SummaryResultModel> summaryResultModel)
        {
            // DELETE old data

            DelSummaryResult(symbolId, datasetId);

            // ADD new data
            foreach (var item in summaryResultModel)
            {
                //var strDate = Convert.ToDateTime(item.Date).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                var startDate = item.StartDate.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                var endDate = item.EndDate.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

                var query = "INSERT IGNORE INTO " + TblSummaryResults;
                query += "(Symbol_ID, Dataset_ID, SymbolName, StartDate, EndDate, Pnl, Reversals, StopLevel, RevLevel, ZIM, PointValue) VALUES";
                query += "(";
                query += symbolId + ",";
                query += datasetId + ",";
                query += "'" + item.SymbolName + "',";
                query += "'" + startDate + "',";
                query += "'" + endDate + "',";
                query += item.Pnl + ",";
                query += item.Reversals + ",";
                query += item.StopLevel + ",";
                query += item.ReversalLevel + ",";
                query += item.Zim + ",";
                query += item.PointValue + ");COMMIT;";

                DoSql(query);
            }
        }

        /// <summary>
        /// Get results
        /// </summary>
        /// <param name="symbolId"></param>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        public static List<SummaryResultModel> GetSummaryResult(int symbolId, int datasetId)
        {
            var result = new List<SummaryResultModel>();

            var sql = "SELECT * FROM " + TblSummaryResults;
            sql += " WHERE Symbol_ID =" + symbolId + " AND Dataset_ID = " + datasetId + " ORDER BY `StartDate`";


            var reader = GetReader(sql);
            if (reader != null)
            {
                while (reader.Read())
                {
                    var srm = new SummaryResultModel
                        {
                            SymbolName = reader.GetString(3), 
                            StartDate = reader.GetDateTime(4),
                            EndDate = reader.GetDateTime(5),
                            Pnl = reader.GetDouble(6),
                            Reversals = reader.GetInt32(7),
                            StopLevel = reader.GetDouble(8),
                            ReversalLevel = reader.GetDouble(9),
                            Zim = reader.GetDouble(10),
                            PointValue = reader.GetDouble(11)
                        };

                    result.Add(srm);
                }

                reader.Close();
            }

            return result;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="symbolId"></param>
        /// <param name="datasetId"></param>
        public static void DelSummaryResult(int symbolId, int datasetId)
        {
            var query1 = "DELETE FROM " + TblSummaryResults;
            query1 += " WHERE Symbol_ID =" + symbolId + " AND Dataset_ID = " + datasetId + " ;COMMIT;";

            DoSql(query1);
        }

        #endregion


        #region  WEEKLYDATADISPLAYER RESULTS
        /// <summary>
        /// Save WeeklyData To DataBase
        /// </summary>
        /// <param name="symbolId"></param>
        /// <param name="datasetId"></param>
        /// <param name="weeklyResultTradeModel"></param>
        public static void AddWeeklyResult(int symbolId, int datasetId, IEnumerable<WeeklyDataTradeModel> weeklyResultTradeModel, IEnumerable<WeeklyDataOrderModel> weeklyDataOrderModel)
        {
            // DELETE old data

            DelWeeklyResult(symbolId, datasetId);

            // ADD new data
            foreach (var item in weeklyResultTradeModel)
            {

                var timeOpen = item.TimeOpen.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                var timeClose = item.TimeClose.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                var comment = "comment";
                var query = "INSERT IGNORE INTO " + TblWeeklyDataTradeResults;
                query += "(Symbol_ID, Dataset_ID, id_Day, TimeOpen, TimeClose, Operation, OpenPr, ClosePr, Trades, Commission, PosPNL, ClosePNL, Comment) VALUES";
                query += "(";
                query += symbolId + ",";
                query += datasetId + ",";
                query += item.id_Day + ",";
                query += "'" + timeOpen + "',";
                query += "'" + timeClose + "',";
                query += "'" + item.Operation + "'" + ",";
                query += "'" + item.OpenPr.ToString("n", CultureInfo.InvariantCulture) + "'" + ",";
                query += "'" + item.ClosePr.ToString("n", CultureInfo.InvariantCulture) + "'" + ",";
                query += "'" + item.Trades.ToString("n", CultureInfo.InvariantCulture) + "'" + ",";
                query += "'" + item.Commission.ToString("n", CultureInfo.InvariantCulture) + "'" + ",";
                query += "'" + item.PosPNL.ToString("n", CultureInfo.InvariantCulture) + "'" + ",";
                query += "'" + item.ClosePNL.ToString("n", CultureInfo.InvariantCulture) + "'" + ",";
                query += comment;
                query += ");";

                DoSql(query);
            }
            foreach (var items in weeklyDataOrderModel)
            {
                var orderTime = items.Time.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                var query2 = "INSERT IGNORE INTO " + TblWeeklyDataOrderResults;
                query2 += "(Symbol_ID, Dataset_ID, Time,Operation,Price) VALUES";
                query2 += "(";
                query2 += symbolId + ",";
                query2 += datasetId + ",";
                query2 += "'" + orderTime + "',";
                query2 += "'" + items.Operation + "'" + ",";
                query2 += "'" + items.Price.ToString("n", CultureInfo.InvariantCulture) + "'" + ");";

                DoSql(query2);
            }
            var qry = "COMMIT;";
            DoSql(qry);
        }

        /// <summary>
        /// GetWeeklyData From DataBase
        /// </summary>
        /// <param name="symbolId"></param>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        public static List<WeeklyDataTradeModel> GetWeeklyDataTradeResult(int symbolId, int datasetId)
        {
            var result = new List<WeeklyDataTradeModel>();

            var sql = "SELECT * FROM " + TblWeeklyDataTradeResults;
            sql += " WHERE Symbol_ID =" + symbolId + " AND Dataset_ID = " + datasetId + " ORDER BY `TimeOpen`";


            var reader = GetReader(sql);
            if (reader != null)
            {
                while (reader.Read())
                {

                    var aid_Day = reader.GetInt32(3);
                    var aTimeOpen = reader.GetDateTime(4);
                    var aTimeClose = reader.GetDateTime(5);
                    var aOperation = reader.GetString(6);
                    var aOpenPr = reader.GetDouble(7);
                    var aClosePr = reader.GetDouble(8);
                    var aTrades = reader.GetDouble(9);
                    var aCommission = reader.GetDouble(10);
                    var aPosPNL = reader.GetDouble(11);
                    var aClosePNL = reader.GetDouble(12);
                    var aComment = "";

                    var srm = new WeeklyDataTradeModel()
                    {
                        id_Day = aid_Day,
                        TimeOpen = aTimeOpen,
                        TimeClose = aTimeClose,
                        Operation = aOperation,
                        OpenPr = aOpenPr,
                        ClosePr = aClosePr,
                        Trades = aTrades,
                        Commission = aCommission,
                        PosPNL = aPosPNL,
                        ClosePNL = aClosePNL,
                        Comment = aComment

                    };


                    result.Add(srm);
                }

                reader.Close();
            }

            return result;
        }

        public static List<WeeklyDataOrderModel> GetWeeklyDataOrderResult(int symbolId, int datasetId)
        {
            var result = new List<WeeklyDataOrderModel>();

            var sql = "SELECT * FROM " + TblWeeklyDataOrderResults;
            sql += " WHERE Symbol_ID =" + symbolId + " AND Dataset_ID = " + datasetId + " ORDER BY `Time`";


            var reader = GetReader(sql);
            if (reader != null)
            {
                while (reader.Read())
                {

                    var wom = new WeeklyDataOrderModel()
                    {

                        Time = reader.GetDateTime(3),
                        Operation = reader.GetString(4),
                        Price = reader.GetDouble(5)

                    };

                    result.Add(wom);
                }

                reader.Close();
            }

            return result;
        }
        /// <summary>
        /// Delete WeeklyData Results From DB
        /// </summary>
        /// <param name="symbolId"></param>
        /// <param name="datasetId"></param>
        public static bool DelWeeklyResult(int symbolId, int datasetId)
        {
            var query1 = "DELETE FROM " + TblWeeklyDataTradeResults;
            query1 += " WHERE Symbol_ID =" + symbolId + " AND Dataset_ID = " + datasetId + " ;COMMIT;";

            var bool1 = DoSql(query1);

            var query2 = "DELETE FROM " + TblWeeklyDataOrderResults;
            query2 += " WHERE Symbol_ID =" + symbolId + " AND Dataset_ID = " + datasetId + " ;COMMIT;";

            var bool2 = DoSql(query2);
            return bool1 || bool2;
        }

        #endregion


        #region COLLECTING


        /// <summary>
        /// Function find first data row and return his DateTime
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static DateTime GetStartDate(string symbol)
        {
            var res = DateTime.Today;
            var sql = "SELECT * FROM " + GetTableNameFromContract(symbol) + " ORDER BY Time ASC LIMIT 1";
            var reader = GetReader(sql);
            if (reader != null)
            {
                if (reader.Read())
                {
                    res = reader.GetDateTime(1);
                }
                reader.Close();
            }

            return res;
        }

        /// <summary>
        /// Function find last data row and return his DateTime
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static DateTime GetEndDate(string symbol)
        {
            var res = DateTime.Today;
            var sql = "SELECT * FROM " + GetTableNameFromContract(symbol) +" ORDER BY Time DESC LIMIT 1";
            var reader = GetReader(sql);
            if (reader != null)
            {
                if (reader.Read())
                {
                    res = reader.GetDateTime(1);
                }
                reader.Close();
            }

            return res;
        }        

        /// <summary>
        /// Add tick data to table
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="date"></param>
        /// <param name="price"></param>
        public static void AddTick(string symbol, DateTime date, double price)
        {
            var dateStr = Convert.ToDateTime(date).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            var sql = "INSERT IGNORE INTO " + GetTableNameFromContract(symbol)
            + " (`Time`,`Price`) "
            + "VALUES('" + dateStr + "', '" + price + "');";

            AddToQueue(sql);                        
        }

        private static void AddToQueue(string sql)
        {
            QueryQueue.Add(sql);
            if (QueryQueue.Count >= 300)
            {
                CommitQueue();
            }
        }

        internal static void CommitQueue()
        {
            if (QueryQueue.Count <= 0) return;

            var fullSql = QueryQueue.Aggregate("", (current, t) => current + t);
            fullSql += "COMMIT;";
            DoSql(fullSql);

            QueryQueue.Clear();
        }

        /// <summary>
        /// Delete contract tick data brtween start and end dates
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public static void DeleteTicks(string contract, DateTime startDate, DateTime endDate)
        {
            var startDateStr = Convert.ToDateTime(startDate).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            var endDateStr = Convert.ToDateTime(endDate).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            var sql = "DELETE FROM `" + GetTableNameFromContract(contract) + "` WHERE `Time` BETWEEN '" + startDateStr + "' AND '" + endDateStr + "';COMMIT;";

            DoSql(sql);
        }

        #endregion


        #region OTHER FUNCTIONS

        /// <summary>
        /// Initialize connection to DB
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="database">Database</param>
        /// <param name="user">User</param>
        /// <param name="password">Password</param>
        /// <returns>Return true if connection success</returns>
        public static bool Initialize(string host, string database, string user, string password)
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                CloseConnection();
            }

            var connectionString = "SERVER=" + host + ";UID=" + user + ";PASSWORD=" + password;
            _connection = new MySqlConnection(connectionString);

            if (OpenConnection())
            {
                CreateDataBase(database);
                if (_connection.State == ConnectionState.Open)
                    _connection.Clone();
            }
            else
                return false;

            var connectionDbString = "SERVER=" + host + ";DATABASE=" + database + ";UID=" + user + ";PASSWORD=" + password;
            _connection = new MySqlConnection(connectionDbString);

            if (OpenConnection())
            {
                CreateTables();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Open connection to db
        /// </summary>
        /// <returns></returns>
        private static bool OpenConnection()
        {
            try
            {
                _connection.Open();

                if (_connection.State == ConnectionState.Open)
                {
                    _sqlCommand = _connection.CreateCommand();
                    _sqlCommand.CommandText = "SET AUTOCOMMIT=0;";
                    _sqlCommand.ExecuteNonQuery();

                    return true;
                }
            }
            catch (MySqlException)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// close connection to db
        /// </summary>
        private static void CloseConnection()
        {
            if ((_connection.State != ConnectionState.Open) || (_connection.State == ConnectionState.Broken)) return;
            _sqlCommand.CommandText = "COMMIT;";
            _sqlCommand.ExecuteNonQuery();
            _connection.Close();
        }

        /// <summary>
        /// create database
        /// </summary>        
        /// <param name="dataBase"></param>
        private static void CreateDataBase(string dataBase)
        {
            var sql = "CREATE DATABASE IF NOT EXISTS `" + dataBase + "`;COMMIT;";
            DoSql(sql);
        }

        /// <summary>
        /// Create tables
        /// </summary>
        private static void CreateTables()
        {
            const string createContractsSql = "CREATE TABLE  IF NOT EXISTS `" + TblContracts + "` ("
                                     + "`ID` INT(10) UNSIGNED  NOT NULL AUTO_INCREMENT,"
                                     + "`ContractName` VARCHAR(50) NULL,"
                                     + "PRIMARY KEY (`ID`,`ContractName`)"
                                     + ")"
                                     + "COLLATE='latin1_swedish_ci'"
                                     + "ENGINE=InnoDB;";
            DoSql(createContractsSql);

            const string createDataSetsSql = "CREATE TABLE  IF NOT EXISTS `" + TblDatasets + "` ("
                                             + "`ID` INT(10) UNSIGNED  NOT NULL AUTO_INCREMENT,"
                                             + "`Symbol_ID` INT(10) NULL,"
                                             + "`DataSetName` VARCHAR(50) NULL,"
                                             + "`TimeValue` FLOAT(9,5) NULL, "
                                             + "`Commission` FLOAT(9,5) NULL, "
                                             + "`Multiplier` INT(12) NULL, "
                                             + "`ContractSize` INT(10) NULL, "
                                             + "`PointValue` FLOAT(9,5) NULL, "
                                             + "`ZIM` FLOAT(9,5) NULL, "
                                             + "`TickSize` FLOAT(9,5) NULL, "

                                             + "`StopLevelDef` FLOAT(9,5) NULL, "
                                             + "`StopLevelMin` FLOAT(9,5) NULL, "
                                             + "`StopLevelMax` FLOAT(9,5) NULL, "
                                             + "`StopLevelStep` FLOAT(9,5) NULL, "

                                             + "`ReversalLevelDef` FLOAT(9,5) NULL, "
                                             + "`ReversalLevelMin` FLOAT(9,5) NULL, "
                                             + "`ReversalLevelMax` FLOAT(9,5) NULL, "
                                             + "`ReversalLevelStep` FLOAT(9,5) NULL, "

                                             + "PRIMARY KEY (`ID`,`DataSetName`)"
                                             + ")"
                                             + "COLLATE='latin1_swedish_ci'"
                                             + "ENGINE=InnoDB;";
            DoSql(createDataSetsSql);

            const string createResultsSql = "CREATE TABLE  IF NOT EXISTS `" + TblResults + "` ("
                                            + "`ID` INT(10) UNSIGNED  NOT NULL AUTO_INCREMENT,"
                                            + "`Symbol_ID` INT(10) NULL,"
                                            + "`Dataset_ID` INT(10) NULL,"
                                            + "`Date` DateTime NULL, "
                                            + "`Pnl` FLOAT(12,2) NULL, "

                                            + "PRIMARY KEY (`ID`)"
                                            + ")"
                                            + "COLLATE='latin1_swedish_ci'"
                                            + "ENGINE=InnoDB;";
            DoSql(createResultsSql);

            const string createSummaryResultSql = "CREATE TABLE  IF NOT EXISTS `" + TblSummaryResults + "` ("
                                                 + "`ID` INT(10) UNSIGNED  NOT NULL AUTO_INCREMENT,"
                                                 + "`Symbol_ID` INT(10) NULL,"
                                                 + "`Dataset_ID` INT(10) NULL,"
                                                 + "`SymbolName` VARCHAR(50) NULL, "
                                                 + "`StartDate` DateTime NULL, "
                                                 + "`EndDate` DateTime NULL, "
                                                 + "`Pnl` FLOAT(12,2) NULL, "
                                                 + "`Reversals` INT(10) NULL, "
                                                 + "`StopLevel` FLOAT(9,5) NULL, "
                                                 + "`RevLevel` FLOAT(9,5) NULL, "

                                                 + "`ZIM` FLOAT(9,5) NULL, "
                                                 + "`PointValue` FLOAT(9,5) NULL, "

                                                 + "PRIMARY KEY (`ID`)"
                                                 + ")"
                                                 + "COLLATE='latin1_swedish_ci'"
                                                 + "ENGINE=InnoDB;";
            DoSql(createSummaryResultSql);

            const string createWeeklyDataTradeResultSql = "CREATE TABLE  IF NOT EXISTS `" + TblWeeklyDataTradeResults + "` ("
                                            + "`ID` INT(10) UNSIGNED  NOT NULL AUTO_INCREMENT,"
                                            + "`Symbol_ID` INT(10) NULL,"
                                            + "`Dataset_ID` INT(10) NULL,"
                                            + "`id_Day` INT(10) NULL, "
                                            + "`TimeOpen` VARCHAR(50) NULL, "
                                            + "`TimeClose` VARCHAR(50) NULL, "
                                            + "`Operation` VARCHAR(50) NULL, "
                                            + "`OpenPr` FLOAT(9,5) NULL, "
                                            + "`ClosePr` FLOAT(9,5) NULL, "
                                            + "`Trades` FLOAT(9,5) NULL, "
                                            + "`Commission` FLOAT(9,5) NULL, "
                                            + "`PosPNL` FLOAT(9,5) NULL, "
                                            + "`ClosePNL` FLOAT(9,5) NULL, "
                                            + "`Comment` VARCHAR(50) NULL, "
                                            + "PRIMARY KEY (`ID`)"
                                            + ")"
                                            + "COLLATE='latin1_swedish_ci'"
                                            + "ENGINE=InnoDB;";
            DoSql(createWeeklyDataTradeResultSql);

            const string createWeeklyDataOrderResultSql = "CREATE TABLE  IF NOT EXISTS `" + TblWeeklyDataOrderResults + "` ("
                                             + "`ID` INT(10) UNSIGNED  NOT NULL AUTO_INCREMENT,"
                                             + "`Symbol_ID` INT(10) NULL,"
                                             + "`Dataset_ID` INT(10) NULL,"
                                             + "`Time` DateTime NULL,"
                                             + "`Operation` VARCHAR(50) NULL,"
                                             + "`Price` FLOAT(9,5) NULL,"
                                             + "PRIMARY KEY (`ID`)"
                                             + ")"
                                             + "COLLATE='latin1_swedish_ci'"
                                             + "ENGINE=InnoDB;";
            DoSql(createWeeklyDataOrderResultSql);


        }


        /// <summary>
        /// execute sql request
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static bool DoSql(String sql)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    OpenConnection();
                }
                _sqlCommand.CommandText = sql;
                _sqlCommand.ExecuteNonQuery();
                return true;
            }
            catch (MySqlException)
            {
                return false;
            }
        }

        /// <summary>
        /// Return reader for input SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static MySqlDataReader GetReader(String sql)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    OpenConnection();
                }

                var command = _connection.CreateCommand();
                command.CommandText = sql;
                var reader = command.ExecuteReader();

                return reader;

            }
            catch (Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// Create "tbl_tick_symbol"
        /// </summary>
        /// <param name="contract"></param>
        public static void CreateTableForContract(string contract)
        {
            var tableName = GetTableNameFromContract(contract);
            var sql = "CREATE TABLE  IF NOT EXISTS `" + tableName + "` ("
                        + "`ID` INT(10) UNSIGNED  NOT NULL AUTO_INCREMENT,"
                        + "`Time` DATETIME NULL,"
                        + "`Price` DOUBLE NULL,"
                        + "PRIMARY KEY (`ID`)"
                        + ")"
                        + "COLLATE='latin1_swedish_ci'"
                        + "ENGINE=InnoDB;";
            DoSql(sql);
        }

        /// <summary>
        /// Function to create table name fromcontract name
        /// (Only chars and digits there are in name of table)
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        private static string GetTableNameFromContract(string contract)
        {
            var res = contract.Where(Char.IsLetterOrDigit).Aggregate("", (current, item) => current + item);
            return "tbl_tick_" + res;
        }        

        /// <summary>
        /// Is connected to database
        /// </summary>
        /// <returns></returns>
        public static bool IsConnected()
        {
            return _connection.State == ConnectionState.Open;
        }        

        /// <summary>
        /// Get datetime pf forst and last tick data for selected symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static bool GetTickStartEndDates(string symbol, out DateTime startDate, out DateTime endDate)
        {
            startDate = GetStartDate(symbol);
            endDate = GetEndDate(symbol);
            return startDate!=endDate;

        }

        /// <summary>
        /// Get all data for selected symbol in selected date
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<TickDataModel> GetTickData(string symbol, DateTime date)
        {
            var contractModelList = new List<TickDataModel>();
            var dateFrom = Convert.ToDateTime(date).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            var dateTo = Convert.ToDateTime(date.AddDays(1).Date).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);


            var sql = "SELECT * FROM " + GetTableNameFromContract(symbol) + " WHERE Time BETWEEN '" + dateFrom + "' AND '" + dateTo + "'";
            var reader = GetReader(sql);
            if (reader != null)
            {
                while (reader.Read())
                {
                    var cm = new TickDataModel { Date = reader.GetDateTime(1), Price = reader.GetDouble(2) };

                    contractModelList.Add(cm);
                }

                reader.Close();
            }
            return contractModelList;
        }
        #endregion

        
    }
}
