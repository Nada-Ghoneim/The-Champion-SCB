using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(IConfiguration configuration)
    {
        // Get the connection string from configuration
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // Method to get a new SQL connection
    private SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }

    // Method to execute a SQL query that returns a DataTable
    public DataTable ExecuteQuery(string query)
    {
        DataTable dataTable = new DataTable();

        using (SqlConnection conn = GetConnection())
        {
            try
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                conn.Open();
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                // Log the error and the query
                Console.WriteLine($"An error occurred while executing the query: {ex.Message}\nQuery: {query}");
                throw; // Re-throw the exception for further handling if necessary
            }
        }

        return dataTable;
    }

    // Method to execute a SQL command that does not return any data
    public int ExecuteNonQuery(string query)
    {
        int affectedRows = 0;

        using (SqlConnection conn = GetConnection())
        {
            try
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                affectedRows = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Log the error and the query
                Console.WriteLine($"An error occurred while executing the non-query: {ex.Message}\nQuery: {query}");
                throw; // Re-throw the exception for further handling if necessary
            }
        }

        return affectedRows;
    }

    // Method to execute a SQL command that returns a single value
    public object ExecuteScalar(string query)
    {
        object result;

        using (SqlConnection conn = GetConnection())
        {
            try
            {
                SqlCommand command = new SqlCommand(query, conn);
                conn.Open();
                result = command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                // Log the error and the query
                Console.WriteLine($"An error occurred while executing the scalar query: {ex.Message}\nQuery: {query}");
                throw; // Re-throw the exception for further handling if necessary
            }
        }

        return result;
    }
    public int ExecuteNonQuery(string query, List<SqlParameter> parameters = null)
    {
        int affectedRows = 0;

        using (SqlConnection conn = GetConnection())
        {
            try
            {
                SqlCommand cmd = new SqlCommand(query, conn);

                // Add parameters if they exist
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                conn.Open();
                affectedRows = cmd.ExecuteNonQuery();
            }
            catch (SqlException sqlEx)
            {
                // Log or throw detailed SQL exception
                throw new Exception($"SQL Error: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                // Log or throw other exceptions
                throw new Exception($"An error occurred while executing the non-query: {ex.Message}", ex);
            }
        }

        return affectedRows;
    }
    public object ExecuteScalar(string query, List<SqlParameter> parameters)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand(query, connection))
            {
                // Add parameters to command
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters.ToArray());
                }

                connection.Open();
                return command.ExecuteScalar(); // This will return the first column of the first row
            }
        }
    }

}
