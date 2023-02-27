using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.ExcelApp.Hubs;
using RabbitMQ.ExcelApp.Models;

namespace RabbitMQ.ExcelApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHubContext<MyHub> _hubContext;

        public FilesController(AppDbContext appDbContext, IHubContext<MyHub> hubContext)
        {
            _appDbContext = appDbContext;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, int fileId)
        {
            //if(file.Length> = 0)
            if (file is not { Length:>0})
            {
                return BadRequest();
            }

            var userFile = await _appDbContext.UserFiles.FirstAsync(f => f.Id == fileId);
            var filePath = userFile?.FileName + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot/files", filePath);

            using FileStream stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed;

            await _appDbContext.SaveChangesAsync();

            //todo: signalr notfication

            await _hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");

            return Ok();
        }
    }
}
