// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
//

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using tracktor.app;
using tracktor.app.Models;
using tracktor.service;

namespace tracktor.app.Controllers
{
    public class TracktorEntryAction
    {
        public int currentTaskID { get; set; }
        public int newTaskID { get; set; }
    }

    [Authorize]
    [Produces("application/json")]
    [Route("api/task")]
    public class TaskController : TracktorControllerBase
    {
        public TaskController(ITracktorService service, UserManager<ApplicationUser> userManager) : base(service, userManager)
        {
        }

        [HttpPost("update")]
        public TTaskDto Update([FromBody]TTaskDto task)
        {
            return _service.UpdateTask(Context, task);
        }

        [HttpPost("stop")]
        public TracktorWebModel Stop([FromBody]TracktorEntryAction actionModel)
        {
            _service.StopTask(Context, actionModel.currentTaskID);
            return GenerateWebModel(true);
        }

        [HttpPost("start")]
        public TracktorWebModel Start([FromBody]TracktorEntryAction actionModel)
        {
            _service.StartTask(Context, actionModel.newTaskID);
            return GenerateWebModel(true);
        }

        [HttpPost("switch")]
        public TracktorWebModel Switch([FromBody]TracktorEntryAction actionModel)
        {
            _service.SwitchTask(Context, actionModel.currentTaskID, actionModel.newTaskID);
            return GenerateWebModel(true);
        }
    }
}
