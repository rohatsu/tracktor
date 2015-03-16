﻿/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="typings/moment/moment.d.ts" />
/// <reference path="typings/bootbox/bootbox.d.ts" />

var padNumber = function (n: number) {
    var s = n.toString();
    if (s.length === 1) {
        return "0" + s;
    }
    return s;
};

var dateTime = function (s: string) {
    if (s == null) {
        return "-";
    }
    var date = new Date(s);
    var dateString =
        padNumber(date.getUTCDate()) + "/" +
        padNumber(date.getUTCMonth() + 1) + " " +
        padNumber(date.getUTCHours()) + ":" +
        padNumber(date.getUTCMinutes());
    return dateString;
}

var timeSpan = function (n: number) {
    var sec_num = parseInt(n.toString(), 10);
    if (sec_num === 0) {
        return "-";
    }
    var hours = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);

    var time = padNumber(hours) + ":" + padNumber(minutes);
    return time;
}

var timeSpanFull = function (n: number) {
    var sec_num = parseInt(n.toString(), 10);
    if (sec_num === 0) {
        return "00:00:00";
    }
    var hours = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    var seconds = Math.floor((sec_num - (hours * 3600) - (minutes * 60)));

    var time = padNumber(hours) + ":" + padNumber(minutes) + ":" + padNumber(seconds);
    return time;
}

var isZero = function (n: number) {
    return (n == 0);
}

var projectMapping = function (data) {
    var self = this;
    ko.mapping.fromJS(data, {}, this);
    self.Contrib = {
        Today: ko.pureComputed(function () {
            return self.TTasks().reduce(function (pv, vc) {
                return pv + vc.Contrib.Today();
            }, 0)
        }, self),
        ThisWeek: ko.pureComputed(function () {
            return self.TTasks().reduce(function (pv, vc) {
                return pv + vc.Contrib.ThisWeek();
            }, 0)
        }, self),
        ThisMonth: ko.pureComputed(function () {
            return self.TTasks().reduce(function (pv, vc) {
                return pv + vc.Contrib.ThisMonth();
            }, 0)
        }, self)
    };
};

var startTask = function (taskId: number) {
    requestData("api/Tracktor/StartTask", "POST", {
        newTaskID: taskId
    }, updateHomeModel);
}

var switchTask = function (taskId: number) {
    requestData("api/Tracktor/SwitchTask", "POST", {
        currentTaskID: statusModel.TTaskInProgress.TTaskID,
        newTaskID: taskId
    }, updateHomeModel);
}

var stopTask = function (taskId: number) {
    requestData("api/Tracktor/StopTask", "POST", {
        currentTaskID: taskId
    }, updateHomeModel);
}

var saveEntry = function () {
    $('#IsDeleted').val("false");
    requestData("api/Tracktor/UpdateEntry", "POST",
        $("#entryEditForm").serialize(),
        function (data) {
            updateHomeModel(data);
            refreshModel();
        });
}

var deleteEntry = function () {
    $('#IsDeleted').val("true");
    requestData("api/Tracktor/UpdateEntry", "POST",
        $("#entryEditForm").serialize(),
        function (data) {
            refreshModel();
        });
}

var rootModel = function (data) {
    ko.mapping.fromJS(data, rootMapping, this)
};

var rootMapping = {
    'Projects': {
        create: function (options) {
            return new projectMapping(options.data);
        }
    },
    create: function (options) {
        var viewModel = new rootModel(options.data);
        viewModel.Contrib = {
            Today: ko.pureComputed(function () {
                return viewModel.Projects().reduce(function (pv, vc) {
                    return pv + vc.Contrib.Today();
                }, 0)
            }, viewModel),
            ThisWeek: ko.pureComputed(function () {
                return viewModel.Projects().reduce(function (pv, vc) {
                    return pv + vc.Contrib.ThisWeek();
                }, 0)
            }, viewModel),
            ThisMonth: ko.pureComputed(function () {
                return viewModel.Projects().reduce(function (pv, vc) {
                    return pv + vc.Contrib.ThisMonth();
                }, 0)
            }, viewModel)
        };
        return viewModel;
    }
};

var summaryModel;
var statusModel;
var entriesModel;
var reportModel;
var editModel;

var viewOptions = {
    showObsolete: ko.observable(false)
};

var updateTitle = function () {
    if (statusModel.InProgress()) {
        document.title = "tr: " + statusModel.LatestEntry.ProjectName() + " / " + statusModel.LatestEntry.TaskName();
    } else {
        document.title = "tracktor (idle)";
    }
}

var bindHomeModel = function (data) {
    summaryModel = ko.mapping.fromJS(data.SummaryModel, rootMapping);
    ko.applyBindings(summaryModel, document.getElementById('SummaryModel'));

    statusModel = ko.mapping.fromJS(data.StatusModel);
    ko.applyBindings(statusModel, document.getElementById('StatusModel'));

    entriesModel = ko.mapping.fromJS(data.EntriesModel);
    ko.applyBindings(entriesModel, document.getElementById('EntriesModel'));

    reportModel = ko.mapping.fromJS(data.ReportModel);
    ko.applyBindings(reportModel, document.getElementById('ReportModel'));

    editModel = ko.mapping.fromJS(data.EditModel);
    ko.applyBindings(editModel, document.getElementById('EditModel'));

    updateTitle();
    $('.curtain').removeClass('curtain');
    $('.nocurtain').remove();
    timerFunc();
};

var updateHomeModel = function (data) {
    if (data.SummaryModel) {
        ko.mapping.fromJS(data.SummaryModel, rootMapping, summaryModel);
    }
    if (data.StatusModel) {
        ko.mapping.fromJS(data.StatusModel, {}, statusModel);
        updateTitle();
    }
    if (data.EntriesModel) {
        ko.mapping.fromJS(data.EntriesModel, {}, entriesModel);
    }
    if (data.ReportModel) {
        ko.mapping.fromJS(data.ReportModel, {}, reportModel);
    }
    if (data.EditModel) {
        $("#EditEndDate").data("DateTimePicker").date(null);
        ko.mapping.fromJS(data.EditModel, {}, editModel);
        // update datepickers
        $("#EditStartDate").data("DateTimePicker").date(moment(editModel.Entry.StartDate()));
        if (editModel.Entry.EndDate() != null) {
            $("#EditEndDate").data("DateTimePicker").date(moment(editModel.Entry.EndDate()));
        }
    }
};

var refreshModel = function () {
    hideReport();
    requestData("api/Tracktor/GetModel", "GET", {}, updateHomeModel);
};

var _urlRoot = "";

var showReport = function () {
    $(".reportcurtain").show();
}

var hideReport = function () {
    $(".reportcurtain").hide();
}

var initializeModel = function (urlRoot: string) {
    _urlRoot = urlRoot;
    hideReport();
    requestData("api/Tracktor/GetModel", "GET", {}, bindHomeModel);
};

var requestData = function (url: string, method: string, data: any, callback: (any) => void) {
    var tokenKey = "TokenKey";
    var token = sessionStorage.getItem(tokenKey);

    if (token === "") {
        alert("Authorization expired, please sign in.");
        window.location.assign(_urlRoot + "Home/SignIn");
    }
    var headers: { [key: string]: any; } = {
        Authorization: 'Bearer ' + token
    };

    var settings: JQueryAjaxSettings = {
        type: method,
        url: _urlRoot + url,
        data: data,
        headers: headers
    };

    disableButtons();
    $.ajax(settings).done(callback);
    enableButtons();
}

var timeTick = 1;

$.ajaxSetup({
    error: function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status == 401) {
            alert("Authorization expired, please sign in.");
            window.location.assign(_urlRoot + "Home/SignIn");
        } else {
            alert("Error: " + textStatus + ": " + errorThrown);
            enableButtons();
        }
    }
});

var timerFunc = function () {
    if (statusModel.InProgress()) {
        summaryModel.Projects().forEach(function (p) {
            p.TTasks().forEach(function (t) {
                if (t.InProgress()) {
                    var today = t.Contrib.Today();
                    var thisWeek = t.Contrib.ThisWeek();
                    var thisMonth = t.Contrib.ThisMonth();
                    t.Contrib.Today(today + timeTick);
                    t.Contrib.ThisWeek(thisWeek + timeTick);
                    t.Contrib.ThisMonth(thisMonth + timeTick);
                }
            });
        });

        entriesModel.Entries().forEach(function (t) {
            if (t.InProgress()) {
                var contrib = t.Contrib();
                t.Contrib(contrib + timeTick);
            }
        });

        var current = statusModel.LatestEntry.Contrib();
        statusModel.LatestEntry.Contrib(current + timeTick);
    }
    setTimeout(timerFunc, 1000);
}

var updateEditingEntry = function (data) {
    entriesModel.EditingEntry(data);
}

var editEntry = function (entryId) {
    requestData("api/Tracktor/GetEntry", "GET", { entryID: entryId }, function (data) {
        updateHomeModel(data);
        $("#entryEditModal").modal();
    });
}

var signOut = function () {
    requestData("api/Account/Logout", "POST", null, function () {
        var tokenKey = "TokenKey";
        var token = sessionStorage.removeItem(tokenKey);

        window.location.assign("/");
    });
}

var disableButtons = function () {
    $("button").prop("disabled", true);
}

var enableButtons = function () {
    $("button").prop("disabled", false);
}

var generateReport = function () {
    var data = {
        year: $('#ReportYear').val(),
        month: $('#ReportMonth').val(),
        projectID: $('#ReportProject').val()
    };
    hideReport();
    requestData("api/Tracktor/GetWebReport", "GET", data,
        function (data) {
            updateHomeModel(data);
            showReport();
        });
}

var downloadCSV = function () {
    window.location.assign(_urlRoot + "/Home/CSV");
}

var updateEditContrib = function () {
    var startDate = moment(editModel.Entry.StartDate());
    var endDate = editModel.Entry.EndDate();
    if (endDate == null || endDate == "Invalid date") {
        endDate = moment();
    }
    else {
        endDate = moment(endDate);
    }
    endDate.subtract(startDate);
    var duration = moment.duration(endDate);
    editModel.Entry.Contrib(duration.asSeconds());
}

var newTask = function (projectId: number) {
    bootbox.prompt({
        message: "",
        title: "Enter name for the new task:",
        animate: false,
        callback: function (result) {
            if (result) {
                requestData("api/Tracktor/UpdateTask", "POST", {
                    TProjectID: projectId,
                    TTaskID: 0,
                    Name: result
                }, refreshModel);
            }
        }
    });
}

var obsoleteTask = function (taskId: number, taskName: string) {
    bootbox.confirm({
        title: "Confirm task removal",
        message: "Are you sure you'd like to remove task " + taskName + "?",
        animate: false,
        callback: function (result) {
            if (result) {
                requestData("api/Tracktor/UpdateTask", "POST", {
                    TTaskID: taskId,
                    IsObsolete: true
                }, refreshModel);
            }
        }
    });
}

var newProject = function () {
    bootbox.prompt({
        message: "",
        title: "Enter name for the new project:",
        animate: false,
        callback: function (result) {
            if (result) {
                requestData("api/Tracktor/UpdateProject", "POST", {
                    Name: result
                }, refreshModel);
            }
        }
    });
}

var obsoleteProject = function (projectId: number, projectName: string) {
    bootbox.confirm({
            title: "Confirm project removal",
            message: "Are you sure you'd like to remove project " + projectName + "?",
            animate: false,
            callback: function (result) {
                if (result) {
                    requestData("api/Tracktor/UpdateProject", "POST", {
                        TProjectID: projectId,
                        IsObsolete: true
                    }, refreshModel);
                }
            }
    });
}

var renameProject = function (projectId: number, projectName: string) {
    bootbox.prompt({
        message: "",
        title: "Enter new name for the project:",
        value: projectName,
        animate: false,
        callback: function (result) {
            if (result) {
                requestData("api/Tracktor/UpdateProject", "POST", {
                    TProjectID: projectId,
                    Name: result
                }, refreshModel);
            }
        }
    });
}

var renameTask = function (taskId: number, taskName: string) {
    bootbox.prompt({
        message: "",
        title: "Enter new name for the task:",
        value: taskName,
        animate: false,
        callback: function (result) {
            if (result) {
                requestData("api/Tracktor/UpdateTask", "POST", {
                    TTaskID: taskId,
                    Name: result
                }, refreshModel);
            }
        }
    });
}
