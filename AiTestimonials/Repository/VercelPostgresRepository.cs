using System.Text.Json;
using AiTestimonials.Models;
using AiTestimonials.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace AiTestimonials.Repository;

public class VercelPostgresRepository
{

    private NpgsqlDataSource _db;

    public VercelPostgresRepository(IOptions<ServiceOptions> serviceOptions)
    {
        var host = serviceOptions.Value.PostgresHost;
        var db = serviceOptions.Value.PostgresDatabase;
        var user = serviceOptions.Value.PostgresUser;
        var pwd = serviceOptions.Value.PostgresPassword;
        var connectionString = $"Host={host};Username={user};Password={pwd};Database={db}";
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        _db = dataSourceBuilder.Build();
    }

    public async Task<bool> TableExistsAsync(string table)
    {
        await using var cmd = _db.CreateCommand($"SELECT EXISTS ( SELECT FROM pg_tables WHERE tablename = '{table}');");
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            return reader.GetBoolean(0);
        }

        return false;
    }

    public async Task CreatTestimonialsTableAsync()
    {
        var tableExists = await TableExistsAsync("testimonials");
        if (!tableExists)
        {

            await using var cmd = _db.CreateCommand(
                $@"
                CREATE TABLE testimonials (
                id SERIAL PRIMARY KEY,
                testimonial JSONB
                );"
            );
            await cmd.ExecuteReaderAsync();
        }
    }

    public async Task<bool> AddTestimonialAsync(TestimonialResult testimonialResult)
    {
        await using var cmd = _db.CreateCommand($"INSERT INTO testimonials (testimonial) VALUES (@value);");
        var param = new NpgsqlParameter() { ParameterName = "value", NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb, Value = testimonialResult };
        cmd.Parameters.Add(param);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await TableExistsAsync("testimonials");
    }

    public async Task<List<TestimonialResult>> GetTestimonialsAsync()
    {

        var tableExists = await TableExistsAsync("testimonials");
        List<TestimonialResult> res = [];

        if (tableExists)
        {
            await using var cmd = _db.CreateCommand("SELECT testimonial FROM testimonials");
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var testimonial = reader.GetFieldValue<TestimonialResult>(0);
                if (testimonial != null)
                    res.Add(testimonial);
            }
        }
        return res;
    }

}

public record BookingEntity
{
    public string? Id { get; init; }
    public string? Date { get; init; }
    public bool? Confirmed { get; init; }
}