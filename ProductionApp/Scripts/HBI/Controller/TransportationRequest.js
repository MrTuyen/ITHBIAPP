/*
    Author: TuyenNV
    Description: TransportationRequest js file
    Date: 20210318
*/


var baseUrl = "/TransportationRequest/";

// Enum action
var Enum_Action = {
    None: 1,
    Approve: 2,
    Reject: 3
}


// General config txtRDVan
$(document).ready(function () {
    // select2
    $("#txtRDVan").select2({
        placeholder: "Select a van",
        allowClear: true,
        width: 'resolve',
        tag: true
    })

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

    //
    var url = location.href;
    if (url.includes('TransportationRequest/Index#trans-')) {
        var transId = url.split('-')[1];
        $("#trans-" + transId).css("background", "#8ac5fe");
    }
});

$(document).on('click', '.dropdown-menu', function (e) {
    e.stopPropagation();
});

// Refresh data
function Refresh() {
    window.location.href = baseUrl + 'Index'
}

// Add new request
function AddNewRequest(e) {
    var today = new Date().toISOString().substring(0, 10);

    var txtPurpose = $("#txtPurpose");
    var txtUsageDate = $("#txtUsageDate");
    var txtTime = $("#txtTime");
    var txtDeparture = $("#txtDeparture");
    var txtArrival = $("#txtArrival");

    // validate
    if (!CheckNullOrEmpty(txtPurpose, "Mục đích không được để trống /Purpose can not be blank"))
        return false;
    if (!CheckNullOrEmpty(txtUsageDate, "Ngày sử dụng không được để trống /Usage date can not be blank"))
        return false;
    //var checkDate = compareDates(today, txtUsageDate.val(), "Ngày sử dụng không được nhỏ hơn ngày hiện tại / Usage date can not smaller than today")
    //if (!checkDate.result) {
    //    toastr.error(checkDate.messErrorForCustomer);
    //    return false;
    //}
    if (!CheckNullOrEmpty(txtTime, "Thời gian đi không được để trống /Departure time can not be blank"))
        return false;
    if (!CheckNullOrEmpty(txtDeparture, "Điểm đi không được để trống /Departure can not be blank"))
        return false;
    if (!CheckNullOrEmpty(txtArrival, "Điểm đến không được để trống /Arrival can not be blank"))
        return false;

    LoadingShow();
    var action = baseUrl + 'AddNewRequest';
    var datasend = JSON.stringify({
        request: {
            Purposes: txtPurpose.val(),
            UsageDate: txtUsageDate.val(),
            DepartureTime: txtTime.val(),
            Departure: txtDeparture.val(),
            Arrival: txtArrival.val()
        }
    });

    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            var item = response.data;
            var html = "<td>" + item.id + "</td>"
                        + "<td>" + item.empId + "</td>"
                        + "<td>" + item.fullName + "</td>"
                        + "<td>" + item.dept + "</td>"
                        + "<td>" + item.usageDate + "</td>"
                        + "<td>" + item.purpose + "</td>"
                        + "<td>" + item.time + "</td>"
                        + "<td>" + item.departure + "</td>"
                        + "<td>" + item.arrival + "</td>"
                        + "<td></td>"
                        + "<td id='mgr-"+item.id+"'><label class='text-danger bold'>Pending</label></td>"
                        + "<td id='hr-" + item.id + "'><label class='text-danger bold'>Pending</label></td>"
                        + "<td><button class='btn btn-primary btn-sm' onclick='GetRequestDetail(" + item.id + ")'>Detail</button></td>";

            $("#transportation-table-body").prepend("<tr>" + html + "</tr>");
            $("#modalAddRequest").modal('hide');
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

function ChangeManagerStatus(requestId, action){
    var ele = $("#mgr-" + requestId);
    ele.html('');
    var html = '';
    switch (action) {
        case Enum_Action.Approve: {
            html = "<label class='text-success bold'>Approved</label>";
        } break;
        case Enum_Action.Reject: {
            html = "<label class='text-danger bold'>Reject</label>";
        } break;
    }
    ele.html(html);
}

// Approve request
function Approve() {
    var requestId = $("#txtRequestId").val();

    var action = baseUrl + 'Approve';
    var datasend = JSON.stringify({
        requestId: requestId
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            ChangeManagerStatus(response.data, Enum_Action.Approve);
            $("#modalUpdateRequest").modal('hide');
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// Reject request
function Reject() {
    var requestId = $("#txtRequestId");
    var reason = $("#txtReason");

    if (!CheckNullOrEmpty(reason, "Lý do không được để trống /Reason can not be blank"))
        return false;

    var action = baseUrl + 'Reject';
    var datasend = JSON.stringify({
        requestId: requestId.val(),
        reason: reason.val()
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            ChangeManagerStatus(response.data, Enum_Action.Reject);
            $("#modalReason").modal('hide');
            $("#modalUpdateRequest").modal('hide');
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// Process request
function Process() {
    var requestId = $("#txtRequestId").val();
    var vanId = $("#txtRDVan");

    if (!vanId.val()) {
        toastr.error("Thông tin xe không được để trống/ Van's info can not be blank");
        return false;
    }

    var action = baseUrl + 'Process';
    var datasend = JSON.stringify({
        requestId: requestId,
        vanId: vanId.val()
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            $("#modalUpdateRequest").modal('hide');
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}


function BindSelect() {
    var action = baseUrl + 'GetListVan';
    var datasend = JSON.stringify({
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            var dataSource = [];
            for (var i = 0; i < response.data.length; i++) {
                var item = response.data[i];
                dataSource.push({ id: item.id, text: item.numberPlate + ' - ' + item.driverName });
            }
            $("#txtRDVan").html('');
            $("#txtRDVan").select2({
                placeholder: "Select a van",
                allowClear: true,
                width: 'resolve',
                tag: true,
                data: dataSource
            });
            $('#txtRDVan').trigger('change');
            LoadingHide();
        }
        else {
            toastr.error(response.msg);
            LoadingHide();
        }
    });
}

// Request detail
function GetRequestDetail(id) {
    var action = baseUrl + 'GetRequestDetail';
    var datasend = JSON.stringify({
        requestId: id
    });

    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            $("#modalUpdateRequest").modal('show');

            var data = response.data;

            $("#txtRequestId").val(data.id);
            $("#txtRDEmpId").val(data.empId);
            $("#txtRDFullname").val(data.fullName);
            $("#txtRDPurpose").val(data.purpose);
            $("#txtRDUsageDate").val(data.usageDate);
            $("#txtRDTime").val(data.time);
            $("#txtRDDeparture").val(data.departure);
            $("#txtRDArrival").val(data.arrival);
            $("#txtRDDepartment").val(data.dept);
            $("#txtRDManager").val(data.mgrName);
            $("#txtRDHrName").val(data.hrName);
            $("#txtRDVan").select2().val(data.van).trigger("change");

            if (data.reason == null || data.reason == "") {
                $("#txtRDReason").parent().css("display", "none");
            }
            else {
                $("#txtRDReason").parent().css("display", "block");
                $("#txtRDReason").val(data.reason);
            }

            var disabled = ($user === data.mgrEmail.toLowerCase() || $username === 'admin') ? "" : "disabled";
            if (disabled != "") {
                $(".btnApprove").attr("disabled", "disabled");
                $(".btnReject").attr("disabled", "disabled");
            }
            else {
                $(".btnApprove").removeAttr("disabled");
                $(".btnReject").removeAttr("disabled");
            }

        }
        else {
            toastr.error(response.msg);
        }
    });
}

// Download report
function Report() {
    LoadingShow();
    var from = $("#txtReportFrom").val();
    var to = $("#txtReportTo").val();

    var action = baseUrl + 'Report';
    var datasend = {
        fromDate: from,
        toDate: to
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
            toastr.error("Something went wrong! Contact IT for more information.")
        }
    })
}

// Add van information
function AddVan() {

    var numberplate = $("#txtNumberPlate");
    var driverName = $("#txtDriverName");
    var driverPhone = $("#txtPhone");

    if (!CheckNullOrEmpty(numberplate, "Biển số không được để trống/ Number plate can not be blank"))
        return false;
    if (!CheckNullOrEmpty(driverName, "Họ tên tài xế không được để trống/ Driver name can not be blank"))
        return false;
    if (!CheckNullOrEmpty(driverPhone, "Số điện thoại tài xế không được để trống/ Driver phone number can not be blank"))
        return false;

    var action = baseUrl + 'AddVan';
    var datasend = JSON.stringify({
        van: {
            numberPlate: numberplate.val(),
            driverName: driverName.val(),
            driverPhone: driverPhone.val()
        }
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            var data = response.data;
            var html = "<tr id='van-" + data.id + "' onclick='GetVanInfo(" + data.id + ")'>"
                        + "<td width='30%'>" + data.numberPlate + "</td>"
                        + "<td width='30%'>" + data.driverName + "</td>"
                        + "<td width='30%'>" + data.phone + "</td>"
                        + "<td class='text-center'>"
                            + "<button class='btn btn-danger btn-sm' onclick='DeleteVan(" + data.id + ")'><i class='fa fa-trash-o'></i></button>"
                        + "</td>"
                        + "</tr>";

            $("#van-table-body").prepend(html);
            toastr.success(response.msg);
            LoadingHide();
        }
        else {
            toastr.error(response.msg);
            LoadingHide();
        }
    });
}

// Update van information
function UpdateVan() {

    var vanId = $("#txtVanId");
    var numberplate = $("#txtNumberPlate");
    var driverName = $("#txtDriverName");
    var driverPhone = $("#txtPhone");

    if (!CheckNullOrEmpty(numberplate, "Biển số không được để trống/ Number plate can not be blank"))
        return false;
    if (!CheckNullOrEmpty(driverName, "Họ tên tài xế không được để trống/ Driver name can not be blank"))
        return false;
    if (!CheckNullOrEmpty(driverPhone, "Số điện thoại tài xế không được để trống/ Driver phone number can not be blank"))
        return false;

    var action = baseUrl + 'UpdateVan';
    var datasend = JSON.stringify({
        van: {
            id: vanId.val(),
            numberPlate: numberplate.val(),
            driverName: driverName.val(),
            driverPhone: driverPhone.val()
        }
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            toastr.success(response.msg);
            LoadingHide();
        }
        else {
            toastr.error(response.msg);
            LoadingHide();
        }
    });
}


// Get van information
function GetVanInfo(id) {
    var action = baseUrl + 'GetVanInfo';
    var datasend = JSON.stringify({
        vanId: id
    });

    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            var data = response.data;
            $("#txtVanId").val(data.id);
            $("#txtNumberPlate").val(data.numberPlate);
            $("#txtDriverName").val(data.driverName);
            $("#txtPhone").val(data.phone);

        }
        else {
            toastr.error(response.msg);
        }
    });
}

// Delete van
function DeleteVan(id) {
    var action = baseUrl + 'DeleteVan';
    var datasend = JSON.stringify({
        vanId: id
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            $("#van-" + id).remove();
            ClearForm();
            toastr.success(response.msg);
            LoadingHide();
        }
        else {
            toastr.error(response.msg);
            LoadingHide();
        }
    });
}

// Get list van
function GetListVan() {
    var action = baseUrl + 'GetListVan';
    var datasend = JSON.stringify({
        
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            var listData = response.data;
            var html = "";

            for (var i = 0; i < listData.length; i++) {
                var data = listData[i];
                html += "<tr id='van-" + data.id + "' onclick='GetVanInfo(" + data.id + ")'>"
                        + "<td width='30%'>" + data.numberPlate + "</td>"
                        + "<td width='30%'>" + data.driverName + "</td>"
                        + "<td width='30%'>" + data.phone + "</td>"
                        + "<td class='text-center'>"
                            + "<button class='btn btn-danger btn-sm' onclick='DeleteVan(" + data.id + ")'><i class='fa fa-trash-o'></i></button>"
                        + "</td>"
                        + "</tr>";
            }
            
            $("#van-table-body").html('');
            $("#van-table-body").prepend(html);
            LoadingHide();
        }
        else {
            toastr.error(response.msg);
            LoadingHide();
        }
    });
}

// Clear van form
function ClearForm() {
    $("#txtVanId").val('');
    $("#txtNumberPlate").val('');
    $("#txtDriverName").val('');
    $("#txtPhone").val('');
}


