/*
    Author: TuyenNV
    Description: RecutController js file
    Date: 20210401
*/
var baseUrl = "/Recut/";

// Action enum
var Enum_Action = {
    None: 1,
    Processing: 2,
    Approve: 3,
    Reject: 4
}

// Action click enum
var Enum_Action_Click = {
    QASew: 1,
    Manager: 2,
    Plan: 3,
    CCDFabric: 4,
    Warehouse: 5,
    CCD: 6,
    QACut: 7,
    Production: 8
}

// Plan update or click
var Plan_Click = {
    Request: 1,
    Data: 2
}

// General config
$(document).ready(function () {
    // select2
    $("#select-group-filter").select2({
        placeholder: "Select a location",
        allowClear: true
    });

    $(".select-location").select2({
        placeholder: "Select a location",
        allowClear: true
    });

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
    window.location.href = '/Recut/Index';
}

function GoBack() {
    window.history.back();
}

function RefreshWoIssue() {
    window.location.href = '/Recut/WOIssues';
}

// Get WO info
function GetWOInfo() {
    if (event.which === 13 || event.key == 'Enter') {
        var wo = $("#txtWO").val();
        LoadingShow();
        var action = baseUrl + 'GetWOInfo';
        var datasend = JSON.stringify({
            wo: wo
        });

        PostDataAjax(action, datasend, function (response) {
            if (response.rs) {
                LoadingHide();

                var data = response.data;
                $("#txtSellingStyle").val(data.sellingStyle);
                $("#txtSize").val(data.size);
                $("#txtMnfColor").val(data.color);
                $("#txtMnfStyle").val(data.mnfStyle);


                var materialCodes = data.listMaterialCode.split(',');
                var html = "";
                for (var i = 0; i < materialCodes.length; i++) {
                    var item = materialCodes[i];
                    html += "<tr>"
                            + "<td><input type='checkbox' class='select-checkbox' data-index='"+i+"' onchange='Select()' /></td>"
                            + "<td id='txtMaterialCode-"+i+"'>" + item + "</td>"
                            + "<td><input type='number' class='form-control' value='0' disabled id='txtTotal-" + i + "' /></td>"
                            + "<td><input type='number' class='form-control' min='0' value='0' oninput='this.value = !!this.value && Math.abs(this.value) >= 0 ? Math.abs(this.value) : null' onchange='CalTotal(" + i + ")' disabled id='txtSewE-" + i + "' /></td>"
                            + "<td><input type='number' class='form-control' min='0' value='0' oninput='this.value = !!this.value && Math.abs(this.value) >= 0 ? Math.abs(this.value) : null' onchange='CalTotal(" + i + ")' disabled id='txtCutE-" + i + "' /></td>"
                            + "<td><input type='number' class='form-control' min='0' value='0' oninput='this.value = !!this.value && Math.abs(this.value) >= 0 ? Math.abs(this.value) : null' onchange='CalTotal(" + i + ")' disabled id='txtSewL-" + i + "' /></td>"
                            + "<td><input type='number' class='form-control' min='0' value='0' oninput='this.value = !!this.value && Math.abs(this.value) >= 0 ? Math.abs(this.value) : null' onchange='CalTotal(" + i + ")' disabled id='txtCutL-" + i + "' /></td>"
                            + "<td><input type='number' class='form-control' min='0' value='0' oninput='this.value = !!this.value && Math.abs(this.value) >= 0 ? Math.abs(this.value) : null' onchange='CalTotal(" + i + ")' disabled id='txtFabricE-" + i + "' /></td>"
                            + "<td><input type='number' class='form-control' min='0' value='0' oninput='this.value = !!this.value && Math.abs(this.value) >= 0 ? Math.abs(this.value) : null' onchange='CalTotal(" + i + ")' disabled id='txtSewTest-" + i + "' /></td>"
                            + "<td><input type='text' class='form-control' disabled id='txtLeader-" + i + "' /></td>"
                        + "</tr>";
                }

                $("#table-material-code").html('');
                $("#table-material-code").html(html);
            }
            else {
                LoadingHide();
                toastr.error(response.msg);
                ResetForm();
            }
        });
    }
}

// Select
function Select() {
    var ele = event.target;
    var id = $(ele).attr("data-index");
    if (ele.checked) {
        Enable(id);
    }
    else {
        Disable(id);
    }
}

//Cal total error and lack
function CalTotal(id) {
    var a = parseInt($("#txtSewE-" + id).val());
    var b = parseInt($("#txtCutE-" + id).val());
    var c = parseInt($("#txtSewL-" + id).val());
    var d = parseInt($("#txtCutL-" + id).val());
    var e = parseInt($("#txtFabricE-" + id).val());
    var f = parseInt($("#txtSewTest-" + id).val());

    if (isNaN(a))
        a = 0;
    if (isNaN(b))
        b = 0;
    if (isNaN(c))
        c = 0;
    if (isNaN(d))
        d = 0;
    if (isNaN(e))
        e = 0;
    if (isNaN(f))
        f = 0;

    var total = a + b + c + d + e + f;
    $("#txtTotal-" + id).val(total);
}

// Disable input
function Disable(id) {
    $("#txtSewE-" + id).attr("disabled", "disabled");
    $("#txtCutE-" + id).attr("disabled", "disabled");
    $("#txtSewL-" + id).attr("disabled", "disabled");
    $("#txtCutL-" + id).attr("disabled", "disabled");
    $("#txtFabricE-" + id).attr("disabled", "disabled");
    $("#txtSewTest-" + id).attr("disabled", "disabled");
    $("#txtLeader-" + id).attr("disabled", "disabled");
}

//  Enable input
function Enable(id) {
    $("#txtSewE-" + id).removeAttr("disabled");
    $("#txtCutE-" + id).removeAttr("disabled");
    $("#txtSewL-" + id).removeAttr("disabled");
    $("#txtCutL-" + id).removeAttr("disabled");
    $("#txtFabricE-" + id).removeAttr("disabled");
    $("#txtSewTest-" + id).removeAttr("disabled");
    $("#txtLeader-" + id).removeAttr("disabled");
}

// Clear form
function ResetForm() {
    $("#table-material-code").html('');
    $("#txtSellingStyle").val("");
    $("#txtSize").val("");
    $("#txtMnfColor").val("");
    $("#txtMnfStyle").val("");
}

// Add request
function AddRequest() {
    var listDetail = [];
    var listCB = $(".select-checkbox");
    for (var i = 0; i < listCB.length; i++) {
        var item = listCB[i];
        if (item.checked) {
            var id = $(item).attr("data-index");
            var obj = {
                RcID: $("#txtSewE-" + id).val(),
                MaterialCode: $("#txtMaterialCode-" + id).text(),
                SewingError: $("#txtSewE-" + id).val(),
                CuttingError: $("#txtCutE-" + id).val(),
                SewingLack: $("#txtSewL-" + id).val(),
                CuttingLack: $("#txtCutL-" + id).val(),
                FabricError: $("#txtFabricE-" + id).val(),
                SewingTest: $("#txtSewTest-" + id).val(),
                LeaderName: $("#txtLeader-" + id).val()
            };

            listDetail.push(obj);
        }
    }

    LoadingShow();
    var action = baseUrl + 'AddRequest';
    var datasend = JSON.stringify({
        request: {
            WO: $("#txtWO").val(),
            SellingStyle: $("#txtSellingStyle").val(),
            Size: $("#txtSize").val(),
            MnfColor: $("#txtMnfColor").val(),
            MnfStyle: $("#txtMnfStyle").val(),
            RequestManager: $("#txtRequestManager").val()
        },
        detail: listDetail
    });

    // Send to server
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            setTimeout(function(){
                Refresh();
            }, 1500);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// Background service
function BackgroundJob() {
    var recutHub = $.connection.signalRConf;
    var userid = sessionStorage.getItem("userNameSS") === null ? "signalR" : sessionStorage.getItem("userNameSS");

    $.connection.hub.qs = { 'username': userid };

    recutHub.client.newMessageReceivedRecut = function (message) {
        var id = message.id;
        switch (message.actionType) {
            case Enum_Action_Click.QASew:
                QASewClickUI(message);
                break;
            case Enum_Action_Click.Manager:
                ManagerClickUI(id, message.status, message.date);
                break;
            case Enum_Action_Click.Plan:
                PlanClickUI(id, message.status, message.date);
                break;
            case Enum_Action_Click.CCDFabric:
                CCDFabricClickUI(id, message.status, message.date);
                break;
            case Enum_Action_Click.Warehouse:
                WHClickUI(id, message.status, message.date);
                break;
            case Enum_Action_Click.CCD:
                CCDClickUI(id, message.status, message.date);
                break;
            case Enum_Action_Click.QACut:
                QACutClickUI(id, message.status, message.date);
                break;
            case Enum_Action_Click.Production:
                break;
            default: Refresh(); break;
        }
    };

    $.connection.hub.start().done(function () {
        console.log("SignalR");
    })
};
BackgroundJob();

/***************************************************
    Logic
****************************************************/
// QCSewClick
function QCSewClick(status) {
    var id = $("#txtRequestId").val();
    LoadingShow();
    var action = baseUrl + 'QCSewClick';
    var datasend = JSON.stringify({
        id: id,
        status: status
    });

    // Send to server
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// ManagerClick
function ManagerClick(status) {
    var id = $("#txtRequestId").val();
    LoadingShow();
    var action = baseUrl + 'ManagerClick';
    var datasend = JSON.stringify({
        id: id,
        status: status
    });

    // Send to server
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// PlanClick
function PlanClick() {
    var id = $("#txtRequestId").val();

    var listDetail = [];
    var listDetailIds = $("#txtListDetailId").val().split(",");
    for (var i = 0; i < listDetailIds.length; i++) {
        var item = listDetailIds[i];

        var obj = {
            ID: item,
            AltMaterialCode: $("#txtAltMaterialCode-" + item).val(),
            WORecut: $("#txtWORecut-" + item).val(),
        };

        listDetail.push(obj);
    }

    LoadingShow();
    var action = baseUrl + 'PlanClick';
    var datasend = JSON.stringify({
        id: id,
        details: listDetail
    });

    // Send to server
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// PlanUpdate
function PlanUpdate(isDataBackup) {
    var id = $("#txtRequestId").val();

    var listDetail = [];
    var listDetailIds = $("#txtListDetailId").val().split(",");
    for (var i = 0; i < listDetailIds.length; i++) {
        var item = listDetailIds[i];

        var obj = {
            ID: item,
            AltMaterialCode: $("#txtAltMaterialCode-" + item).val(),
            WORecut: $("#txtWORecut-" + item).val(),
        };

        listDetail.push(obj);
    }
    
    LoadingShow();
    var action = baseUrl + 'PlanUpdate';
    var datasend = JSON.stringify({
        id: id,
        details: listDetail,
        isData: isDataBackup
    });

    // Send to server
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// CCDRequestClick
function CCDRequestClick() {
    var id = $("#txtRequestId").val();

    var listDetail = [];
    var listDetailIds = $("#txtListDetailId").val().split(",");
    for (var i = 0; i < listDetailIds.length; i++) {
        var item = listDetailIds[i];

        var obj = {
            ID: item,
            CCDQty: $("#txtCcdQty-" + item).val(),
        };

        if (isNaN(parseInt(obj.CCDQty)) || parseInt(obj.CCDQty) <= 0) {
            toastr.error("Số lượng không được nhỏ hơn 0/ Quantity must be greater than 0.");
            $("#txtCcdQty-" + item).focus();
            return false;
        }

        listDetail.push(obj);
    }

    LoadingShow();
    var action = baseUrl + 'CCDRequestClick';
    var datasend = JSON.stringify({
        id: id,
        details: listDetail
    });

    // Send to server
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// WarehouseClick
function WarehouseClick(status) {
    var id = $("#txtRequestId").val();

    var listDetail = [];
    var listDetailIds = $("#txtListDetailId").val().split(",");
    for (var i = 0; i < listDetailIds.length; i++) {
        var item = listDetailIds[i];

        var obj = {
            ID: item,
            WHQty: $("#txtWHQty-" + item).val(),
        };

        if (isNaN(parseInt(obj.WHQty)) || parseInt(obj.WHQty) <= 0) {
            toastr.error("Số lượng không được nhỏ hơn 0/ Quantity must be greater than 0.");
            $("#txtWHQty-" + item).focus();
            return false;
        }

        listDetail.push(obj);
    }

    LoadingShow();
    var action = baseUrl + 'WarehouseClick';
    var datasend = JSON.stringify({
        id: id,
        status: status,
        details: listDetail
    });

    // Send to server
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// WarehouseClick
function CCDApproveClick() {
    var id = $("#txtRequestId").val();

    var listDetail = [];
    var listDetailIds = $("#txtListDetailId").val().split(",");
    for (var i = 0; i < listDetailIds.length; i++) {
        var item = listDetailIds[i];

        var obj = {
            ID: item,
            CcdConfirm: $("#txtCcdConfirm-" + item).is(':checked'),
        };

        if (obj.CcdConfirm == 0) {
            toastr.error("Các chi tiết cần xác nhận đầy đủ mới có thể approve/ All components must be checked.");
            return false;
        }

        listDetail.push(obj);
    }

    LoadingShow();
    var action = baseUrl + 'CCDApproveClick';
    var datasend = JSON.stringify({
        id: id,
        details: listDetail
    });

    // Send to server
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// QCCutClick
function QCCutClick(status) {
    var id = $("#txtRequestId").val();

    LoadingShow();
    var action = baseUrl + 'QCCutClick';
    var datasend = JSON.stringify({
        id: id,
        status: status
    });

    // Send to server
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// Production Click
function ProductionClick(status) {
    var id = $("#txtRequestId").val();

    LoadingShow();
    var action = baseUrl + 'ProductionClick';
    var datasend = JSON.stringify({
        id: id
    });

    // Send to server
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}


/****************************************************
    Change UI area
****************************************************/
// QCSewClickUI
function QASewClickUI(message) {
    var id = message.id;
    var circle = $("#qasew-circle-" + id);
    var color = "white";
    if (message.status == Enum_Action.Approve)
    {
        color = "green";
        ManagerClickUI(id, Enum_Action.Processing);
    }
    else
    {
        color = "red";
        ManagerClickUI(id, Enum_Action.None);
    }
    circle.css("background", color);
    $("#qasew-date-" + id).text(message.date);
}

// ManagerClickUI
function ManagerClickUI(id, status, date) {
    var circle = $("#manager-circle-" + id);

    var color = "white";
    if (status == Enum_Action.Approve)
    {
        color = "green";
        PlanClickUI(id, Enum_Action.Processing);
    }
    else if (status == Enum_Action.Reject)
    {
        color = "red";
        PlanClickUI(id, Enum_Action.None);
    }
    else if (status == Enum_Action.Processing)
    {
        color = "yellow";
    }

    circle.css("background", color);
    $("#manager-date-" + id).text(date);
}

// PlanClickUI
function PlanClickUI(id, status, date) {
    var circle = $("#plan-circle-" + id);

    var color = "white";
    if (status == Enum_Action.Approve) {
        color = "green";
        CCDFabricClickUI(id, Enum_Action.Processing);
    }
    else if (status == Enum_Action.Reject) {
        color = "red";
        CCDFabricClickUI(id, Enum_Action.None);
    }
    else if (status == Enum_Action.Processing) {
        color = "yellow";
    }

    circle.css("background", color);
    $("#plan-date-" + id).text(date);
}

// CCDFabricClickUI
function CCDFabricClickUI(id, status, date) {
    var circle = $("#ccdfabric-circle-" + id);

    var color = "white";
    if (status == Enum_Action.Approve) {
        color = "green";
        WHClickUI(id, Enum_Action.Processing);
    }
    else if (status == Enum_Action.Reject) {
        color = "red";
        WHClickUI(id, Enum_Action.None);
    }
    else if (status == Enum_Action.Processing) {
        color = "yellow";
    }

    circle.css("background", color);
    $("#ccdfabric-date-" + id).text(date);
}

// WHClickUI
function WHClickUI(id, status, date) {
    var circle = $("#wh-circle-" + id);

    var color = "white";
    if (status == Enum_Action.Approve) {
        color = "green";
        CCDClickUI(id, Enum_Action.Processing);
    }
    else if (status == Enum_Action.Reject) {
        color = "red";
        CCDClickUI(id, Enum_Action.None);
    }
    else if (status == Enum_Action.Processing) {
        color = "yellow";
    }

    circle.css("background", color);
    $("#wh-date-" + id).text(date);
}

// CCDClickUI
function CCDClickUI(id, status, date) {
    var circle = $("#ccd-circle-" + id);

    var color = "white";
    if (status == Enum_Action.Approve) {
        color = "green";
        QACutClickUI(id, Enum_Action.Processing);
    }
    else if (status == Enum_Action.Reject) {
        color = "red";
        QACutClickUI(id, Enum_Action.None);
    }
    else if (status == Enum_Action.Processing) {
        color = "yellow";
    }

    circle.css("background", color);
    $("#ccd-date-" + id).text(date);
}

// QACutClickUI
function QACutClickUI(id, status, date) {
    var circle = $("#qacut-circle-" + id);

    var color = "white";
    if (status == Enum_Action.Approve) {
        color = "green";
        ProductionClickUI(id, Enum_Action.Processing);
    }
    else if (status == Enum_Action.Reject) {
        color = "red";
        ProductionClickUI(id, Enum_Action.None);
    }
    else if (status == Enum_Action.Processing) {
        color = "yellow";
    }

    circle.css("background", color);
    $("#qacut-date-" + id).text(date);
}

// ProductionClickUI
function ProductionClickUI(id, status, date) {
    var circle = $("#production-circle-" + id);

    var color = "white";
    if (status == Enum_Action.Approve) {
        color = "green";
    }
    else if (status == Enum_Action.Reject) {
        color = "red";
    }
    else if (status == Enum_Action.Processing) {
        color = "yellow";
    }

    circle.css("background", color);
    $("#production-date-" + id).text(date);
}

// Download report
function Report() {
    LoadingShow();
    var from = $("#txtReportFrom").val();
    var to = $("#txtReportTo").val();

    var action = baseUrl + 'Report';
    var datasend = {
        fromDate: from,
        toDate: to,
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
            toastr.error("Error on Report. Recut controller.")
        }
    })
}
