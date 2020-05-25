﻿// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
//

using Microsoft.AspNetCore.Http;
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
using tracktor.app.Models;
using tracktor.service;

namespace tracktor.app.Controllers
{
    public class TracktorControllerBase : Controller
    {
        protected ITracktorService _service;
        protected UserManager<ApplicationUser> _userManager;

        public TracktorControllerBase(ITracktorService service, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        public TimeZoneInfo GetUserTimezone(HttpContext httpContext, out int userID)
        {
            var user = _userManager.GetUserAsync(httpContext.User).Result;
            userID = user.TUserID;
            var userTimeZone = TimeZoneInfo.Utc;
            if (!string.IsNullOrWhiteSpace(user.TimeZone))
            {
                userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
            }
            return userTimeZone;
        }

        public TContextDto GetContext(HttpContext httpContext)
        {
            int userID;
            var userTimeZone = GetUserTimezone(httpContext, out userID);
            return new TContextDto
            {
                TUserID = userID,
                UTCOffset = -(int)userTimeZone.GetUtcOffset(DateTime.UtcNow).TotalMinutes
            };
        }

        protected TContextDto Context
        {
            get
            {
                return GetContext(Request.HttpContext);
            }
        }

        protected TracktorWebModel GenerateWebModel(bool updateOnly = false)
        {
            var summaryModel = _service.GetSummaryModel(Context);
            var lookback = DateTime.Today.AddMonths(-1);
            return new TracktorWebModel
            {
                SummaryModel = summaryModel,
                EntriesModel = _service.GetEntriesModel(Context, new DateTime(lookback.Year, lookback.Month, 1).AddDays(-1).ToLocalTime(), null, 0, 0, 999),
                StatusModel = _service.GetStatusModel(Context),
                EditModel = updateOnly ? null : new TEditModelDto
                {
                    Entry = new TEntryDto
                    {
                        EndDate = DateTime.UtcNow,
                        StartDate = DateTime.UtcNow,
                        IsDeleted = false,
                        InProgress = false,
                        TaskName = "",
                        ProjectName = "",
                        TTaskID = 0,
                        TEntryID = 0,
                        Contrib = 0,
                        TProjectID = 0
                    }
                },
                ReportModel = updateOnly ? null : WebReportModel.Create(summaryModel, DateTime.UtcNow)
            };
        }
    }
}
