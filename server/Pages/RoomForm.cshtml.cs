using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace server.Pages;

public class RoomFormModel : PageModel
{

    private readonly ILogger<RoomFormModel> _logger;
    private readonly IRoomService roomService;

    public string Title => "Изменение";
    [BindProperty]
    public Room Room{get;set;}=new Room{Id=0, Name=""};

    public RoomFormModel(ILogger<RoomFormModel> logger, IRoomService roomService)
    {
        _logger = logger;
        this.roomService = roomService;
    }

    public void OnGet(int? id=null)
    {
        //Rooms = roomService.GetRooms();
        if(id!=null){
            var tmpRoom=roomService.GetRooms().FirstOrDefault(room=>room.Id==id);
            if(tmpRoom!=null){
                Room=tmpRoom;
            }
        }

        
    }
    public IActionResult OnPost()
    {
        _logger.LogInformation("Сохраняем"+Room.Name);
        if(Room.Id==0){
            roomService.InsertData(Room);
        }
        else{
            roomService.UpdateData(Room);
        }
        return RedirectToPage("./Administration");
    }
}

