/*
    Author: TuyenNV
    Description: RecutController js file
    Date: 20210401
*/
var baseUrl = "/Travel/";

// Action enum
var Enum_Action = {
    None: 1,
    Processing: 2,
    Approve: 3,
    Reject: 4
}

// Enum order type
var Enum_Type = {
    Hotel: 1,
    Ticket: 2
}

//  
var Enum_Manager_Type = {
    Manager: 1,
    SeniorManager: 2
}

// General config
$(document).ready(function () {
    // select2
    $(".travel").select2({
        placeholder: "Select a destination",
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

    // Highlight the specific request
    var url = location.href;
    if (url.includes('Travel/Index#travel-')) {
        var travelId = url.split('-')[1];
        $("#travel-" + travelId).css("background", "#8ac5fe");
    }
});

$(document).on('click', '.dropdown-menu', function (e) {
    e.stopPropagation();
});

// Refresh data
function Refresh() {
    window.location.href = '/Travel/Index';
}   

function GoBack() {
    window.history.back();
}

// Upload file excel
function UploadFile(type) {
    var e = event;
    var fileName = e.target.files[0].name;

    switch (type) {
        case Enum_Type.Hotel: {
            $('.fileUploadNameHotel').text(fileName);
        } break;
        case Enum_Type.Ticket: {
            $('.fileUploadNameTicket').text(fileName);
        } break;
    }

    if (window.FormData !== undefined) {

        var fileUpload = "";

        switch (type) {
            case Enum_Type.Hotel: {
                fileUpload = $("#fileHotelUpload").get(0);
            } break;
            case Enum_Type.Ticket: {
                fileUpload = $("#fileTicketUpload").get(0);
            } break;
        }
        var files = fileUpload.files;

        // Create FormData object
        var fileData = new FormData();

        // Looping over all files and add it to FormData object
        for (var i = 0; i < files.length; i++) {
            fileData.append(files[i].name, files[i]);
        }

        // Adding one more key to FormData object  
        fileData.append('type', type);

        $.ajax({
            url: baseUrl + 'UploadFile',
            method: 'POST',
            contentType: false,
            processData: false,
            data: fileData,
            success: function (result) {
                if (result.rs) {

                }
                else {
                    toastr.error(result.msg);
                }
            },
            error: function (err) {
                toastr.error(err.statusText);
            }
        });
    } else {
        toastr.error("FormData is not supported.");
    }
}

// Add new request
function AddNewRequest(e) {

    var txtPurpose = $("#txtPurpose");
    var txtDestination = $("#txtDestination");
    var txtDeptDate = $("#txtDeptDate");
    var txtDeptFrom = $("#txtDeptFrom");
    var txtDeptTo = $("#txtDeptTo");
    var txtReturnDate = $("#txtReturnDate");
    var txtReturnFrom = $("#txtReturnFrom");
    var txtReturnTo = $("#txtReturnTo");

    // validate
    if (!CheckNullOrEmpty(txtPurpose, "Mục đích không được để trống / Purpose can not be blank"))
        return false;
    if (!CheckNullOrEmpty(txtDestination, "Điểm đến không được để trống / Destination can not be blank"))
        return false;
    if (!CheckNullOrEmpty(txtDeptDate, "Ngày đi không được để trống / Departure date can not be blank"))
        return false;
    if (!CheckNullOrEmpty(txtDeptFrom, "Điểm đi chặng đi không được để trống / Departure location can not be blank"))
        return false;
    if (!CheckNullOrEmpty(txtDeptTo, "Điểm đến chặng đi không được để trống / Departure destination can not be blank"))
        return false;
    if (!CheckNullOrEmpty(txtReturnDate, "Ngày về không được để trống / Return date can not be blank"))
        return false;
    if (!CheckNullOrEmpty(txtReturnFrom, "Điểm đi chặng về không được để trống / Return location can not be blank"))
        return false;
    if (!CheckNullOrEmpty(txtReturnTo, "Điểm đến chặng về không được để trống / Return destination can not be blank"))
        return false;

    LoadingShow();
    var action = baseUrl + 'AddNewRequest';
    var datasend = JSON.stringify({
        request: {
            Purpose: txtPurpose.val(),
            Destination: txtDestination.val(),
            DepartureDate: txtDeptDate.val(),
            DepartureFrom: txtDeptFrom.val(),
            DepartureTo: txtDeptTo.val(),
            ReturnDate: txtReturnDate.val(),
            ReturnFrom: txtReturnFrom.val(),
            ReturnTo: txtReturnTo.val()
        }
    });

    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            var item = response.data;
            var html = "<td>" + item.id + "</td>"
                        + "<td>" + item.name + " - " + item.position + "</td>"
                        + "<td>" + item.department + "</td>"
                        + "<td>" + item.purpose + "</td>"
                        + "<td></td>"
                        + "<td class='text-danger bold'>Pending</td>"
                        + "<td class='text-danger bold'>Pending</td>"
                        + "<td class='text-danger bold'>Pending</td>"
                        + "<td><button class='btn btn-primary btn-sm' onclick='GetRequestDetail(" + item.id + ")'>Detail</button></td>";

            $("#travel-table-body").prepend("<tr>" + html + "</tr>");

            toastr.success(response.msg);
            $("#modalAddRequest").modal('hide');
            LoadingHide();
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
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
            $("#txtRDName").val(data.name + " - " + data.position);
            $("#txtRDPurpose").val(data.purpose);

            $("#txtRDDeptDate").val(data.deptDate);
            $("#txtRDDeptFrom").val(data.deptFrom);
            $("#txtRDDeptTo").val(data.deptTo);

            $("#txtRDReturnDate").val(data.returnDate);
            $("#txtRDReturnFrom").val(data.returnFrom);
            $("#txtRDReturnTo").val(data.returnTo);

            $("#txtRDDepartment").val(data.dept);
            $("#txtRDManager").val(data.manager);
            $("#txtRDSManager").val(data.seniorManager);
            $("#txtRDHR").val(data.hrName);

            if (data.note == null || data.note == "") {
                $("#txtRDReason").parent().css("display", "none");
            }
            else {
                $("#txtRDReason").parent().css("display", "block");
                $("#txtRDReason").val(data.note);
            }

            var disabled = ($user === data.managerMail.toLowerCase() || $username === 'admin') ? "" : "disabled";
            if (disabled != "") {
                $(".btnApprove").attr("disabled", "disabled");
                $(".btnReject").attr("disabled", "disabled");
            }
            else {
                $(".btnApprove").removeAttr("disabled");
                $(".btnReject").removeAttr("disabled");
            }

            disabled = ($user === data.seniorManagerMail.toLowerCase() || $username === 'admin') ? "" : "disabled";
            if (disabled != "") {
                $(".btnSApprove").attr("disabled", "disabled");
                $(".btnSReject").attr("disabled", "disabled");
            }
            else {
                $(".btnSApprove").removeAttr("disabled");
                $(".btnSReject").removeAttr("disabled");
            }
        }
        else {
            toastr.error(response.msg);
        }
    });
}

// Add destination
function AddDestination() {
    var error = $("#txtDesError");
    var name = $("#txtDestinationName");
    if (!CheckNullOrEmpty(name, "Tên địa điểm không được để trống/ Destination name can not be blank"))
        return false;

    var action = baseUrl + 'AddDestination';
    var datasend = JSON.stringify({
        name: name.val()
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            toastr.success(response.msg);
            LoadingHide();
            name.val('');
            $(".isError").text('');
            $("#modalAddDestination").modal("hide");
        }
        else {
            error.text(response.msg);
            LoadingHide();
        }
    });
}

function BindSelect() {
    var action = baseUrl + 'GetListDestination';
    var datasend = JSON.stringify({
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            var dataSource = [];
            for (var i = 0; i < response.data.length; i++) {
                var item = response.data[i];
                dataSource.push({ id: item.name, text: item.name});
            }
            $(".travel").html('');
            $(".travel").select2({
                placeholder: "Select a destination",
                allowClear: true,
                width: 'resolve',
                tag: true,
                data: dataSource
            });
            $('.travel').trigger('change');
            LoadingHide();
        }
        else {
            toastr.error(response.msg);
            LoadingHide();
        }
    });
}

// Manager Approve request
function Approve(type) {
    var requestId = $("#txtRequestId").val();

    var action = baseUrl + 'ManagerApprove';
    if (type == Enum_Manager_Type.SeniorManager) {
        action = baseUrl + 'SManagerApprove';
    }
    var datasend = JSON.stringify({
        requestId: requestId
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            $("#modalUpdateRequest").modal('hide');

            if (type == Enum_Manager_Type.SeniorManager) {
                ChangeSManagerStatus(response.data, Enum_Action.Approve);
            }
            else {
                ChangeManagerStatus(response.data, Enum_Action.Approve);
            }
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// Manager Reject request
function Reject(type) {
    var requestId = $("#txtRequestId");
    var reason = $("#txtReason");  

    var action = baseUrl + 'ManagerReject';
    if (type == Enum_Manager_Type.SeniorManager) {
        reason = $("#txtSReason");
        action = baseUrl + 'SManagerReject';
    }
    if (!CheckNullOrEmpty(reason, "Lý do không được để trống /Reason can not be blank"))
        return false;

    var datasend = JSON.stringify({
        requestId: requestId.val(),
        note: reason.val()
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            $("#modalUpdateRequest").modal('hide');

            if (type == Enum_Manager_Type.SeniorManager) {
                ChangeSManagerStatus(response.data, Enum_Action.Reject);
                $("#modalSReason").modal('hide');
            }
            else {
                ChangeManagerStatus(response.data, Enum_Action.Reject);
                $("#modalReason").modal('hide');
            }
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

    var action = baseUrl + 'Process';
    var datasend = JSON.stringify({
        requestId: requestId
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

function ChangeManagerStatus(requestId, action) {
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

function ChangeSManagerStatus(requestId, action) {
    var ele = $("#smgr-" + requestId);
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

function ConfigChange() {
    var isChecked = $("#txtDelegate").is(':checked');

    var action = baseUrl + 'Delegate';
    var datasend = JSON.stringify({
        isChecked: isChecked
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            $("#modalConfig").modal('hide');
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}
