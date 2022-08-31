/*
    Author: TuyenNV
    Description: AutoKanbanController js file
    Date: 20210308
*/
var baseUrl = "/AutoKanban/";

// Action enum
var Enum_Action = {
    Cancel: 1,
    Call: 2,
    CPSend: 3,
    SPSend: 4,
    Complete: 5
}

// SetInterval or ClearInterval Enum
var Enum_Interval = {
    SetInterval: 1,
    ClearInterval: 2
}

var Enum_View_Type = {
    Called: 1,
    Complete: 2
}

// Interval array
var arrInterval = [];

// General config
$(document).ready(function () {
    // select2
    $("#select-group-filter").select2({
        placeholder: "Select a location",
        allowClear: true
    });

    //$(".select-location").select2({
    //    placeholder: "Select a location",
    //    allowClear: true
    //});

    $("#txtLocation").select2({
        placeholder: "Select a location",
        allowClear: true,
        width: 'resolve'
    });

    $('.modal').on('shown.bs.modal', function () {
        $(this).find('[autofocus]').focus();
    });

    // datetime picker
    $('.isDate').datepicker({
        format: "mm/dd/yyyy",
    });

    $(document).on('click', '.day', function (e) {
        $('.datepicker').css('display', 'none')
        e.preventDefault();
        e.stopPropagation();
    })
});

$(document).on('click', '.dropdown-menu', function (e) {
    e.stopPropagation();
});

// Refresh data
function Refresh() {
    window.location.href = '/AutoKanban/Index';
}

// Upload file excel
function UploadExcel() {
    var e = event;
    var fileName = e.target.files[0].name;
    $('.fileUploadName').text(fileName);

    if (window.FormData !== undefined) {

        var fileUpload = $("#fileAutoKanbanUpload").get(0);
        var files = fileUpload.files;

        // Create FormData object
        var fileData = new FormData();

        // Looping over all files and add it to FormData object
        for (var i = 0; i < files.length; i++) {
            fileData.append(files[i].name, files[i]);
        }

        LoadingShow();
        $.ajax({
            url: baseUrl + 'UploadFile',
            method: 'POST',
            contentType: false,
            processData: false,
            data: fileData,
            timeout: 0,
            success: function (result) {
                if (result.rs) {
                    var listSheet = JSON.parse(result.listSheet);
                    var options = "";
                    for (var i = 0; i < listSheet.length; i++) {
                        let item = listSheet[i];
                        options += "<option value="+item.Index+">" + item.SheetName + "</option>";
                    }
                    
                    $(".selected-sheet").html("").append(options);
                    $(".selected-header").focus();
                    console.log(result.msg);
                    LoadingHide();
                }
                else {
                    toastr.error(result.msg);
                    LoadingHide();
                }
            },
            error: function (err) {
                LoadingHide();
                //toastr.error(err.statusText);
            }
        });
    } else {
        toastr.error("FormData is not supported.");
    }
}

// Save data from excel file
function UploadKanban() {
    LoadingShow();
    var action = baseUrl + 'ReadExcelFile';
    var datasend = JSON.stringify({
        selectedSheet: $(".selected-sheet").val(),
        selectedHeader: $(".selected-header").val()
    });

    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            $('#modalUploadKanban').modal("hide");
            if (response.file) {
                toastr.options = {
                    "closeButton": true,
                    "progressBar": true,
                    "timeOut": "10000",
                    "positionClass": "toast-top-center",
                }
                toastr.success('Tải file lỗi <a href="' + response.file + '" download="error_import_file.txt">tại đây</a>', 'Thành công');
            }
            else {
                toastr.success(response.msg);
            }
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}   

// Filter by group
function GroupFilter() {
    $("#btnKanban").click();
}

// Count time run every 1 minute
function RunTime(assWo, clickTime) {
    var actionTime = $("#action-time-" + assWo);
    var time = clickTime;
    actionTime.text(time);
    if (time > 240) {
        actionTime.addClass("text-danger");
    }
    function Timer() {
        time++;
        actionTime.text(time);
        if (time > 240) {
            actionTime.addClass("text-danger");
        }
    }

    var myInterval = setInterval(Timer, 1000 * 60);
    arrInterval.push({ assWo: assWo, id : myInterval });
}

// Clear interval
function ClearTime(assWo) {
    var actionTime = $("#action-time-" + assWo);
    actionTime.text(0);
    actionTime.removeClass("text-danger");
    if (arrInterval.length > 0) {
        let intervalId = arrInterval.filter(function (x) {
            return x.assWo.toString() === assWo;
        });

        if (intervalId.length > 0) {
            clearInterval(intervalId[0].id);
        }
    }
}

//
function OpenCancelModal(assWo) {
    $("#txtWo").val(assWo);
    $("#modalReason").modal('show');
}

// Action
function Action(actionType){
    var ele = $(event.target);
    var assWo = "";
    var cancelReason = "";
    if (actionType == Enum_Action.Cancel) {
        assWo = $("#txtWo").val();
        cancelReason = $("#txtReason").val();
    }
    else {
        assWo = ele.attr("data-asswo");
    }
    var location = $("#select-location-" + assWo).val();
    var actionTime = $("#action-time-" + assWo).text();

    // Call to server
    LoadingShow();
    var action = baseUrl + 'Action';
    var datasend = JSON.stringify({
        assWo: assWo,
        action: actionType,
        location: location,
        actionTime: actionTime,
        cancelReason: cancelReason
    });

    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);

            switch (actionType) {
                case Enum_Action.Cancel: {
                    $("#modalReason").modal('hide');
                    $("#txtWo").val('');
                    $("#txtReason").val('');
                } break;
                case Enum_Action.SPSend: {
                    $("#tr-" + assWo).remove();
                } break;
                case Enum_Action.CPSend: {
                    $("#tr-" + assWo).remove();
                } break;
            }

            //if (actionType == Enum_Action.Cancel) {
            //    $("#modalReason").modal('hide');
            //    $("#txtWo").val('');
            //    $("#txtReason").val('');
            //}
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// Call click: Change CP and SP to red
function Call(assWo, message) {
    var row = document.getElementById("tr-" + assWo); // find row to copy
    var table = document.getElementById("table-kanban-body"); // find table to append to
    var clone = row.cloneNode(true); // copy children too
    $("#tr-" + assWo).remove();
    if (message.newestAssWo.length > 0) {
        $(clone).insertAfter("#tr-" + message.newestAssWo);
    }
    else {
        table.prepend(clone);
    }

    //$("#select-location-" + assWo).val(message.locationId);
    //$("#td-" + assWo)[0].lastElementChild.remove();

    $("#call-date-" + assWo).text(message.callDate);
    ClearTime(assWo);
    RunTime(assWo, 0);
    CPChange(assWo, "red");
    SPChange(assWo, "red");
}

// CP click: Change CP to yellow
function CPSend(assWo){
    CPChange(assWo, "yellow");
}

// SP click: Change SP to yellow
function SPSend(assWo) {
    SPChange(assWo, "yellow");
}

// Cancel click: Change both CP and SP to white
function Cancel(assWo) {
    $("#call-date-" + assWo).text("");
    ClearTime(assWo);
    CPChange(assWo, "white");
    SPChange(assWo, "white");
}

// Complete click: Save data row to TBL_KANBAN_DATA
function Complete(assWo) {
    $("#tr-" + assWo).remove();
}

// CP change color
function CPChange(assWo, color) {
    $("#cp-circle-" + assWo).css("background", color);
}

// SP change color
function SPChange(assWo, color) {
    $("#sp-circle-" + assWo).css("background", color);
}

// Background service
function BackgroundJob() {
    var kanbanHub = $.connection.signalRConf;
    var userid = sessionStorage.getItem("userNameSS") === null ? "signalR" : sessionStorage.getItem("userNameSS");

    $.connection.hub.qs = { 'username': userid };

    kanbanHub.client.newMessageReceived = function (message) {
        let assWo = message.assWo;
        switch (message.actionType) {
            case Enum_Action.Cancel:
                Cancel(assWo);  
                break;
            case Enum_Action.Call:                
                Call(assWo, message);
                break;
            case Enum_Action.CPSend:
                CPSend(assWo);
                break;
            case Enum_Action.SPSend:
                SPSend(assWo);
                break;
            case Enum_Action.Complete:
                Complete(assWo);
                break;
            default: Refresh(); break;
        }
    };

    $.connection.hub.start().done(function () {
        console.log("SignalR");
    })
};
BackgroundJob();

// Report view change
function RadioChange(viewType) {
    if (viewType == Enum_View_Type.Complete) {
        $(".report-complete").css("display", "block");
    }
    else {
        $(".report-complete").css("display", "none");
    }
}

// Download report
function Report() {
    LoadingShow();
    var from = $("#txtReportFrom").val();
    var to = $("#txtReportTo").val();
    var location = $("#txtLocation").val();
    var reportType = $("input[name='report']:checked").val();

    var action = baseUrl + 'Report';
    var datasend = {
        fromDate: from,
        toDate: to,
        location: location === null ? 0 : location,
        reportType: reportType
    };
    $.fileDownload(action, {
        httpMethod: "POST",
        data: datasend,
        successCallback: function (url) {
            LoadingHide();
        },
        prepareCallback: function (url) {
        },
        failCallback: function (response) {
            LoadingHide();
            toastr.error("Không có work order nào hoàn thành. / There are no work order completed.")
        }
    })
}

function BindSelect(assWo, selectedId) {
    var action = baseUrl + 'GetListLocation';
    var datasend = JSON.stringify({
    });
    //LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            var dataSource = [];
            for (var i = 0; i < response.data.length; i++) {
                var item = response.data[i];
                dataSource.push({ id: item.id, text: item.name });
            }
           
            $('#select-location-' + assWo).html('');
            $('#select-location-' + assWo).select2({
                placeholder: "Select a location",
                allowClear: true,
                width: 'resolve',
                tag: true,
                data: dataSource
            }).val(selectedId).trigger("change");

            
            //LoadingHide();
        }
        else {
            toastr.error(response.msg);
            //LoadingHide();
        }
    });
}