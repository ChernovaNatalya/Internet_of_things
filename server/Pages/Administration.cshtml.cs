using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace server.Pages;

public class AdministrationModel : PageModel
{

    private readonly ILogger<AdministrationModel> _logger;
    private readonly IRoomService roomService;

    public string Title => "Администрирование";

    public Room[] Rooms { get; private set; } = [];

    public AdministrationModel(ILogger<AdministrationModel> logger, IRoomService roomService)
    {
        _logger = logger;
        this.roomService = roomService;
    }

    public void OnGet()
    {
        Rooms = roomService.GetRooms();
    }
    public IActionResult OnPostDelete(int id)
    {
        _logger.LogInformation("Удаляем " + id);
        roomService.DeleteRoom(id);
        return RedirectToPage("./Administration");
    }
}

