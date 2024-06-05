using Npgsql;
using server;
using server.Pages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHostedService<TimerService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IDeviceDataStorage, DeviceDataFileStorage>();
builder.Services.AddScoped<IRoomService, TestRoomService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

public interface IDeviceDataStorage
{
    void SaveData(int roomId, DateTime dateTime, DeviceData data);
    (DateTime date, DeviceData data)[] GetLastData(int roomId);
}

// public class DeviceDataLogStorage(ILogger<DeviceDataLogStorage> logger) : IDeviceDataStorage
// {
//     public void SaveData(int roomId, DateTime dateTime, DeviceData data)
//     {
//         logger.LogInformation($"Device data from room {roomId}: {data}");
//     }
// }

public class DeviceDataFileStorage : IDeviceDataStorage
{
    private readonly string ConnectionString = "Server=localhost;Port=5432;User Id=postgres;Password=123;Database=internet_of_thing;";

    public (DateTime date, DeviceData data)[] GetLastData(int roomId)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var cmd = new NpgsqlCommand();
        cmd.Connection = connection;
        cmd.CommandText = $"SELECT * FROM device_data WHERE id_room={roomId} ORDER BY date DESC LIMIT 10";
        NpgsqlDataReader reader = cmd.ExecuteReader();
        //var result = new DeviceData[];
        var result = new List<(DateTime date, DeviceData data)>();
        while(reader.Read()){
            var date = reader.GetDateTime(2);
            var temp = reader.GetFloat(3);
            var hum = reader.GetFloat(4);
            var light = reader.GetFloat(5);
            result.Add(
                (date: date,
                data: new DeviceData(temp, hum, light)));
        }
        connection.Close();
        return result.ToArray();
    }

    public void SaveData(int roomId, DateTime dateTime, DeviceData data)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var cmd = new NpgsqlCommand();
        cmd.Connection = connection;
        cmd.CommandText = $"INSERT INTO device_data (id_room, date, temperature, humidity, light) VALUES (@id_room, @date, @temperature, @humidity, @light)";
        cmd.Parameters.AddWithValue("id_room", roomId);
        cmd.Parameters.AddWithValue("date", dateTime);
        cmd.Parameters.AddWithValue("temperature", data.Temperature);
        cmd.Parameters.AddWithValue("humidity", data.Humidity);
        cmd.Parameters.AddWithValue("light", data.Light);
        cmd.ExecuteNonQuery();
        connection.Close();
    }
}

public interface IRoomService
{
    Room[] GetRooms();
    void DeleteRoom(int roomId);
    void InsertData(Room room);
    void UpdateData(Room room);
}

public class TestRoomService : IRoomService
{
    private readonly string ConnectionString = "Server=localhost;Port=5432;User Id=postgres;Password=123;Database=internet_of_thing;";

    public Room[] GetRooms(){
    
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var cmd = new NpgsqlCommand();
        cmd.Connection = connection;
        cmd.CommandText = $"SELECT * FROM rooms ORDER BY id";
        NpgsqlDataReader reader = cmd.ExecuteReader();
        //var result = new DeviceData[];
        var result = new List<Room>();
        while(reader.Read()){
            result.Add( new Room{
                Id= reader.GetInt32(0), 
                Name= reader.GetString(1),
                MinTemp=reader.GetFloat(2),
                MaxTemp=reader.GetFloat(3),
                MinHum=reader.GetFloat(4),
                MaxHum=reader.GetFloat(5),
                MinLight=reader.GetFloat(6),
                MaxLight=reader.GetFloat(7),
            });
        }
        connection.Close();
        return result.ToArray();
    }

    public void DeleteRoom(int roomId){
         using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var cmd = new NpgsqlCommand();
        cmd.Connection = connection;
        cmd.CommandText = $"DELETE FROM rooms WHERE id={roomId}";
        cmd.ExecuteNonQuery();
        connection.Close();
    }


    public void InsertData(Room room)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var cmd = new NpgsqlCommand();
        cmd.Connection = connection;
        cmd.CommandText = "INSERT INTO rooms (name, temp_min, temp_max, hum_min, hum_max, light_min, light_max) VALUES (@name, @min_temp, @max_temp, @min_hum, @max_hum, @min_light, @max_light)";
        // Установка параметров команды
        cmd.Parameters.AddWithValue("@name", room.Name);
        cmd.Parameters.AddWithValue("@min_temp", (float)room.MinTemp);
        cmd.Parameters.AddWithValue("@max_temp", (float)room.MaxTemp);
        cmd.Parameters.AddWithValue("@min_hum", (float)room.MinHum);
        cmd.Parameters.AddWithValue("@max_hum", (float)room.MaxHum);
        cmd.Parameters.AddWithValue("@min_light", (float)room.MinLight);
        cmd.Parameters.AddWithValue("@max_light", (float)room.MaxLight);
        cmd.ExecuteNonQuery();
        connection.Close();
    }
    public void UpdateData(Room room)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var cmd = new NpgsqlCommand();
        cmd.Connection = connection;
        cmd.CommandText = $@"UPDATE rooms SET name=@name, temp_min=@temp_min, temp_max=@temp_max, hum_min=@hum_min, 
        hum_max=@hum_max, light_min=@light_min, light_max=@light_max WHERE id=@id";
        cmd.Parameters.AddWithValue("id", room.Id);
        cmd.Parameters.AddWithValue("name", room.Name);
        cmd.Parameters.AddWithValue("temp_min", room.MinTemp);
        cmd.Parameters.AddWithValue("temp_max", room.MaxTemp);
        cmd.Parameters.AddWithValue("hum_min", room.MinHum);
        cmd.Parameters.AddWithValue("hum_max", room.MaxHum);
        cmd.Parameters.AddWithValue("light_min", room.MinLight);
        cmd.Parameters.AddWithValue("light_max", room.MaxLight);
        cmd.ExecuteNonQuery();
        connection.Close();
    }
}

public class Room{
    public int Id{get;set;}
    public string Name{get;set;}
    public float MinTemp{get;set;}
    public float MaxTemp{get;set;}
    public float MinHum{get;set;}
    public float MaxHum{get;set;}
    public float MinLight{get;set;}
    public float MaxLight{get;set;}
    };