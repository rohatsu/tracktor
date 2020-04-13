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
    [Authorize]
    [Produces("application/json")]
    [Route("api/project")]
    public class ProjectController : TracktorControllerBase
    {
        public ProjectController(ITracktorService service, UserManager<ApplicationUser> userManager) : base(service, userManager)
        {
        }

        [HttpPost("update")]
        public TProjectDto Update([FromBody]TProjectDto project)
        {
            return _service.UpdateProject(Context, project);
        }
    }
}
