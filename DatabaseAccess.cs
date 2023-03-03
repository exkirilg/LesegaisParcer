using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LesegaisParcer
{
    public class DatabaseAccess
    {
        private const string _connectionString
            = "Server=DESKTOP-BDPEF9E;Database=lesegaisParcerDb;Trusted_Connection=True;Trust Server Certificate=true;";

        public void EnsureTableExists()
        {
            SqlCommand command = new SqlCommand(
                @"
                IF OBJECT_ID('Deals', 'U') IS NULL
	            CREATE TABLE Deals
	            (
                    DealNumber NVARCHAR(50) PRIMARY KEY,
                    DealDate DATE,
                    SellerName NVARCHAR(250),
                    SellerInn NVARCHAR(25),
                    BuyerName NVARCHAR(250),
                    BuyerInn NVARCHAR(25),
                    WoodVolumeBuyer FLOAT,
                    WoodVolumeSeller FLOAT
	            )"
            );

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                command.Connection = connection;
                command.ExecuteNonQuery();
            }
        }

        public void ProcessData(IEnumerable<Deal> data)
        {
            var fData = FilterData(data);

            var getCommand = GetByDealNumberCommand(fData);
            var insertCommand = InsertCommand();
            var updateCommand = UpdateCommand();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                getCommand.Connection = connection;
                var deals = GetDealsFromDb(getCommand);

                insertCommand.Connection = connection;
                updateCommand.Connection = connection;

                foreach (var deal in fData)
                {
                    if (deals.Where(d => d.DealNumber == deal.DealNumber).Any())
                    {
                        var pDeal = deals.Where(d => d.DealNumber == deal.DealNumber).First();
                        if (pDeal.DealDate <= deal.DealDate 
                            && (deal.SellerName != pDeal.SellerName || deal.SellerInn != pDeal.SellerInn
                            || deal.BuyerName != pDeal.BuyerName || deal.BuyerInn != pDeal.BuyerInn
                            || deal.WoodVolumeSeller != pDeal.WoodVolumeBuyer || deal.WoodVolumeBuyer != pDeal.WoodVolumeBuyer))
                        {
                            SetSqlCommandParams(updateCommand, deal);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        SetSqlCommandParams(insertCommand, deal);
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        private IEnumerable<Deal> FilterData(IEnumerable<Deal> data)
        {
            return data
                .Where(d =>
                    !string.IsNullOrWhiteSpace(d.DealNumber)
                    && d.DealDate > new DateTime(2020, 1, 1)
                    && d.DealDate < new DateTime(DateTime.Now.Year + 1, 1, 1)
                )
                .GroupBy(d => d.DealNumber)
                .Select(g => new Deal()
                {
                    DealNumber = g.Key,
                    DealDate = g.Max(d => d.DealDate),
                    SellerName = g.Max(d => d.SellerName),
                    SellerInn = g.Max(d => d.SellerInn),
                    BuyerName = g.Max(d => d.BuyerName),
                    BuyerInn = g.Max(d => d.BuyerInn),
                    WoodVolumeSeller = g.Max(d => d.WoodVolumeSeller),
                    WoodVolumeBuyer = g.Max(d => d.WoodVolumeBuyer)
                })
                .ToArray();
        }

        private SqlCommand GetByDealNumberCommand(IEnumerable<Deal> data)
        {
            SqlCommand command = new SqlCommand();

            var ids = data.Select(d => d.DealNumber).ToArray();
            string[] parameters = new string[data.Count()];

            for (int i = 0; i < data.Count(); i++)
            {
                parameters[i] = $"@num{i}";
                command.Parameters.AddWithValue(parameters[i], ids[i]);
            }

            command.CommandText = $"SELECT * FROM Deals WHERE DealNumber IN ({string.Join(",", parameters)})";

            return command;
        }

        private IEnumerable<Deal> GetDealsFromDb(SqlCommand getCommand)
        {
            SqlDataReader reader = getCommand.ExecuteReader();

            if (!reader.HasRows)
            {
                reader.Close();
                return Enumerable.Empty<Deal>();
            }
            
            List<Deal> deals = new List<Deal>();
            while (reader.Read())
            {
                deals.Add(new Deal
                {
                    DealNumber = reader.GetString(0),
                    DealDate = reader.GetDateTime(1),
                    SellerName = reader.GetString(2),
                    SellerInn = reader.GetString(3),
                    BuyerName = reader.GetString(4),
                    BuyerInn = reader.GetString(5),
                    WoodVolumeSeller = reader.GetFloat(6),
                    WoodVolumeBuyer = reader.GetFloat(7)
                });
            }

            reader.Close();

            return deals;
        }

        private SqlCommand InsertCommand()
        {
            SqlCommand command = new SqlCommand(
                @"
                INSERT INTO Deals
                    (DealNumber, DealDate, SellerName, SellerInn, BuyerName, BuyerInn, WoodVolumeBuyer, WoodVolumeSeller)
                VALUES
                    (@DealNumber, @DealDate, @SellerName, @SellerInn, @BuyerName, @BuyerInn, @WoodVolumeBuyer, @WoodVolumeSeller)"
            );

            command.Parameters.AddRange(new SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@DealNumber" },
                new SqlParameter() { ParameterName = "@DealDate" },
                new SqlParameter() { ParameterName = "@SellerName" },
                new SqlParameter() { ParameterName = "@SellerInn" },
                new SqlParameter() { ParameterName = "@BuyerName" },
                new SqlParameter() { ParameterName = "@BuyerInn" },
                new SqlParameter() { ParameterName = "@WoodVolumeBuyer" },
                new SqlParameter() { ParameterName = "@WoodVolumeSeller" }
            });

            return command;
        }

        private SqlCommand UpdateCommand()
        {
            SqlCommand command = new SqlCommand(
                @"
                UPDATE Deals SET
                    DealDate = @DealDate,
                    SellerName = @SellerName,
                    SellerInn = @SellerInn,
                    BuyerName = @BuyerName,
                    BuyerInn = @BuyerInn,
                    WoodVolumeBuyer = @WoodVolumeBuyer,
                    WoodVolumeSeller = @WoodVolumeSeller
                WHERE DealNumber = @DealNumber"
            );

            command.Parameters.AddRange(new SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@DealNumber" },
                new SqlParameter() { ParameterName = "@DealDate" },
                new SqlParameter() { ParameterName = "@SellerName" },
                new SqlParameter() { ParameterName = "@SellerInn" },
                new SqlParameter() { ParameterName = "@BuyerName" },
                new SqlParameter() { ParameterName = "@BuyerInn" },
                new SqlParameter() { ParameterName = "@WoodVolumeBuyer" },
                new SqlParameter() { ParameterName = "@WoodVolumeSeller" }
            });

            return command;
        }

        private void SetSqlCommandParams(SqlCommand command, Deal deal)
        {
            command.Parameters[command.Parameters.IndexOf("@DealNumber")].Value = deal.DealNumber;
            command.Parameters[command.Parameters.IndexOf("@DealDate")].Value = deal.DealDate;
            command.Parameters[command.Parameters.IndexOf("@SellerName")].Value = deal.SellerName;
            command.Parameters[command.Parameters.IndexOf("@SellerInn")].Value = deal.SellerInn;
            command.Parameters[command.Parameters.IndexOf("@BuyerName")].Value = deal.BuyerName;
            command.Parameters[command.Parameters.IndexOf("@BuyerInn")].Value = deal.BuyerInn;
            command.Parameters[command.Parameters.IndexOf("@WoodVolumeBuyer")].Value = deal.WoodVolumeBuyer;
            command.Parameters[command.Parameters.IndexOf("@WoodVolumeSeller")].Value = deal.WoodVolumeSeller;
        }
    }
}
