using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace server.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IRoomService roomService;
    public readonly IDeviceDataStorage deviceDataStorage;

    public string Title => "Показатели датчиков";

    public Room[] Rooms { get; private set; }
    public List<RoomData> RoomDatas { get; } = new List<RoomData>();


    public IndexModel(ILogger<IndexModel> logger, IRoomService roomService, IDeviceDataStorage deviceDataStorage)
    {
        _logger = logger;
        this.roomService = roomService;
        this.deviceDataStorage = deviceDataStorage;

    }

    public void OnGet()
    {
        Rooms = roomService.GetRooms();
        foreach (var room in roomService.GetRooms())
        {
            var roomData = new RoomData();
            roomData.RoomId = room.Id;
            roomData.RoomName = room.Name;
            roomData.TemperatureInterval =$"{room.MinTemp}-{room.MaxTemp}";
            roomData.HumidityInterval =$"{room.MinHum}-{room.MaxHum}";
            roomData.LightInterval =$"{room.MinLight}-{room.MaxLight}";
            var lastData = this.deviceDataStorage.GetLastData(roomData.RoomId);
            foreach (var rec in lastData)
            {
                var record = new RoomDataRecord
                {
                    Date = rec.date,
                    Temperature = rec.data.Temperature,
                    TemperatureColor = rec.data.Temperature > room.MinTemp && rec.data.Temperature < room.MaxTemp ? "green" : "red",
                    Humidity = rec.data.Humidity,
                    HumidityColor = rec.data.Humidity > room.MinHum && rec.data.Humidity < room.MaxHum ? "green" : "red",
                    Light = rec.data.Light,
                    LightColor = rec.data.Light > room.MinLight && rec.data.Light < room.MaxLight ? "green" : "red"
                };
                roomData.Records.Add(record);
            }
            RoomDatas.Add(roomData);

        }

        //FillFakeData();
    }

    private void FillFakeData()
    {
        var roomDataId = 1;
        var roomDataName = "abc";
        var fakeDate = DateTime.Now;
        var t = 10;
        var h = 10;
        var l = 10;

        var room1 = new RoomData
        {
            RoomId = roomDataId,
            RoomName = roomDataName
        };
        room1.Records.Add(new RoomDataRecord
        {
            Date = fakeDate,
            Temperature = t,
            Humidity = h,
            Light = l
        });
        room1.Records.Add(new RoomDataRecord
        {
            Date = fakeDate,
            Temperature = t,
            Humidity = h,
            Light = l
        });

        var room2 = new RoomData
        {
            RoomId = roomDataId,
            RoomName = roomDataName
        };
        room2.Records.Add(new RoomDataRecord
        {
            Date = fakeDate,
            Temperature = t,
            Humidity = h,
            Light = l
        });
        room2.Records.Add(new RoomDataRecord
        {
            Date = fakeDate,
            Temperature = t,
            Humidity = h,
            Light = l
        });

        RoomDatas.Add(room1);
        RoomDatas.Add(room2);
    }
}

public class RoomData
{
    public int RoomId { get; set; }
    public string RoomName { get; set; }
    public List<RoomDataRecord> Records { get; } = new List<RoomDataRecord>();
    public string TemperatureInterval{get;set;}
    public string HumidityInterval{get;set;}
    public string LightInterval{get;set;}
}

public class RoomDataRecord
{
    public DateTime Date { get; set; }
    public float Temperature { get; set; }
    public string TemperatureColor { get; set; }
    public float Humidity { get; set; }
    public string HumidityColor { get; set; }
    public float Light { get; set; }
    public string LightColor { get; set; }

}