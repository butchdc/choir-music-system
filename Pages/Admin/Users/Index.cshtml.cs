using choir_music_system.Data;
using choir_music_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Pages.Admin.Users;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ChoirDbContext _db;

    public IndexModel(ChoirDbContext db)
    {
        _db = db;
    }

    public List<AppUser> Users { get; set; } = new();

    public async Task OnGetAsync()
    {
        Users = await _db.AppUsers
            .OrderBy(x => x.Email)
            .ToListAsync();
    }
}