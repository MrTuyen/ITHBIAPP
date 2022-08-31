/*
    Author: TuyenNV
    Description: Meal order js file
    Date: 20210329
*/

var baseUrl = "/MealOrder/";
var domain = "http://10.113.97.26/";

// Enum action
var Enum_Action = {
    None: 1,
    Approve: 2,
    Reject: 3
}

// Enum order type
var Enum_Type = {
    Milk: 1,
    Water: 2
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
    if (url.includes('MealOrder/Index#meal-')) {
        var transId = url.split('-')[1];
        $("#meal-" + transId).css("background", "#8ac5fe");
    }
});

$(document).on('click', '.dropdown-menu', function (e) {
    e.stopPropagation();
});

// Refresh data
function Refresh() {
    window.location.href = baseUrl + 'Index'
}

// Upload file excel
function UploadFile(type) {
    var e = event;
    var fileName = e.target.files[0].name;

    switch (type) {
        case Enum_Type.Milk: {
            $('.fileUploadNameMilk').text(fileName);
        } break;
        case Enum_Type.Water: {
            $('.fileUploadNameWater').text(fileName);
        } break;
    }

    if (window.FormData !== undefined) {

        var fileUpload = "";

        switch (type) {
            case Enum_Type.Milk: {
                fileUpload = $("#fileMilkUpload").get(0);
            } break;
            case Enum_Type.Water: {
                fileUpload = $("#fileWaterUpload").get(0);
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
    var txtShift = $("#txtShift");
    var txtQtyMeal = $("#txtQtyMeal");
    var txtQtyMilk = $("#txtQtyMilk");
    var txtQtyWater = $("#txtQtyWater");

    //validate
    if (!CheckNullOrEmpty(txtQtyMeal, "Số lượng suất ăn không được để trống/ Arrival can not be blank"))
        return false;
    if (parseInt(txtQtyMeal.val()) <= 0) {
        toastr.error("Số lượng suất ăn không nhỏ hơn 0/ Quantity of meals is not less than 0");
        return false;
    }

    LoadingShow();
    var action = baseUrl + 'AddNewRequest';
    var datasend = JSON.stringify({
        request: {
            Shift: txtShift.val(),
            QtyMeal: txtQtyMeal.val(),
            QtyMilk: txtQtyMilk.val(),
            QtyWater: txtQtyWater.val(),
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
                        + "<td>" + item.shift + "</td>"
                        + "<td>" + item.qtyMeal + "</td>"
                        + "<td>" + item.qtyMilk + "</td>"
                        + "<td>" + item.qtyWater + "</td>"
                        + "<td>" + item.total + "</td>"
                        + "<td id='hr-" + item.id + "'><label class='text-danger bold'>Pending</label></td>"
                        + "<td><button class='btn btn-primary btn-sm' onclick='GetRequestDetail(" + item.id + ")'>Detail</button></td>";

            $("#mealorder-table-body").prepend("<tr>" + html + "</tr>");
            $("#modalAddRequest").modal('hide');
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
    var comment = $("#txtRDComment");

    // validate
    if (!CheckNullOrEmpty(comment, "Ghi chú không được để trống/ Comment can not be blank"))
        return false;

    var action = baseUrl + 'Process';
    var datasend = JSON.stringify({
        requestId: requestId,
        comment: comment.val()
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
            $("#txtRDDepartment").val(data.dept);
            $("#txtRDHrName").val(data.hrName);
            $("#txtRDShift").val(data.shift);
            $("#txtRDQtyMeal").val(data.qtyMeal);
            $("#txtRDQtyMilk").val(data.qtyMilk);
            $("#txtRDQtyWater").val(data.qtyWater);
            $("#txtRDQtyTotal").val(data.total);
            $("#txtRDComment").val(data.comment);

            $("#txtMilkFilePath").html('');
            if (data.milkFilePath.length > 0) {
                $("#txtMilkFilePath").html("<a class='btn btn-info btn-sm' href='" + domain + "Uploads/mealorder/" + data.milkFilePath + "' target='_blank'>Tải xuống/ Download</a>");
            }

            $("#txtWaterFilePath").html('');
            if (data.waterFilePath.length > 0) {
                $("#txtWaterFilePath").html("<a class='btn btn-info btn-sm' href='" + domain + "Uploads/mealorder/" + data.waterFilePath + "' target='_blank'>Tải xuống/ Download</a>");
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
