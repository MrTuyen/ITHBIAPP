/*
    Author: TuyenNV
    Description: ITAssetsController js file
    Date: 20210225
*/
var baseUrl = "/ITAssets/";

var asset_id = $("#txtID");
var tag = $("#txtTag");
var serial = $("#txtSerial");
var model = $("#txtModel");
var user = $("#txtUser");
var txt_status = $("#txtStatus");
var dept = $("#select-department");
var division = $("#select-division");
var purDate = $("#txtPurDate");
var warrantyDate = $("#txtWarrantyDate");
var countSheet = $("#txtCountsheet");
var note = $("#txtNote");
var lbTotal = $(".lbTotal");
var tableBody = $("#it-asset-content");

// General config
$(document).ready(function () {
    // select2
    $("#select-department").select2({
        placeholder: "Select a Department",
        allowClear: true
    });

    $("#select-dept-filter").select2({
        placeholder: "Select a Department",
        allowClear: true
    });

    $("#select-division").select2({
        placeholder: "Select a Division",
        allowClear: true
    });

    $("#select-div-filter").select2({
        placeholder: "Select a Division",
        allowClear: true
    });

    $("#txtModel").select2({
        placeholder: "Select a Model",
        allowClear: true
    });

    $('.modal').on('shown.bs.modal', function () {
        $(this).find('[autofocus]').focus();
    });

    // datetime picker
    $('.isDate').datepicker({ dateFormat: "mm/dd/yy" });    
});


$(document).on('click', '.dropdown-menu', function (e) {
    e.stopPropagation();
});

function ResetControl() {
    asset_id.val('');
    tag.val('');
    serial.val('');
    user.val('');
    note.val('');
    txt_status.val('');
    countSheet.val('');
    purDate.val('');
    warrantyDate.val('');

    tag.focus();
}

function ScanTag() {
    if (event.which === 13 || event.key == 'Enter') {
        let obj = {};
        let tagValue = tag.val();
        if (tag.val().length > 0) {
            // Send request to get asset info
            var action = baseUrl + 'GetAssetByTag';
            var datasend = JSON.stringify({
                data: {
                    tag: tagValue
                }
            });
            LoadingShow();
            PostDataAjax(action, datasend, function (response) {
                if (response.rs) {
                    LoadingHide();
                    obj = response.data;
                }
                else {
                    LoadingHide();
                    toastr.error(response.msg);
                }
            });

            // asign data
            setTimeout(function () {
                asset_id.val(obj.ID);
                tag.val(obj.TAG);
                serial.val(obj.SERIAL);
                model.select2().val(obj.MODEL).trigger("change");
                division.select2().val(obj.DIVISION).trigger("change");
                dept.select2().val(obj.DEPT).trigger("change");
                user.val(obj.USER);
                note.val(obj.NOTE);
                if (obj.STATUS === null || obj.STATUS === '' || obj.STATUS.length <= 0) {
                    txt_status.val(moment(new Date()).format("DD/MM/YYYY"));
                }
                else {
                    txt_status.val(obj.STATUS);
                }
                if (obj.COUNTSHEET === null || obj.COUNTSHEET === '' || obj.COUNTSHEET.length <= 0) {
                    countSheet.val("Countsheet tiếp theo nè");
                }
                else {
                    countSheet.val(obj.COUNTSHEET);
                }
                purDate.val(obj.PUR_DATE);
                warrantyDate.val(obj.WARRANTY);

                user.focus();
            }, 200)
        }
        else {
            toastr.error("Bạn chưa nhập tag. /Tag can not blank.");
        }
    }
}

// Refresh data
function ITAssetRefresh() {
    window.location.href = '/ITAssets/CheckingAssets';
}

// Upload file excel
function UploadExcel() {
    var e = event;
    var fileName = e.target.files[0].name;
    $('.fileUploadName').text(fileName);

    if (window.FormData !== undefined) {

        var fileUpload = $("#fileITAssetsUpload").get(0);
        var files = fileUpload.files;

        // Create FormData object
        var fileData = new FormData();

        // Looping over all files and add it to FormData object
        for (var i = 0; i < files.length; i++) {
            fileData.append(files[i].name, files[i]);
        }

        $.ajax({
            url: baseUrl + 'UploadFile',
            method: 'POST',
            contentType: false,
            processData: false,
            data: fileData,
            success: function (result) {
                if (result.rs) {
                    var listSheet = JSON.parse(result.listSheet);
                    var options = "";
                    for (var i = 0; i < listSheet.length; i++) {
                        let item = listSheet[i];
                        options += "<option value=" + item.Index + ">" + item.SheetName + "</option>";
                    }

                    $(".selected-sheet").html("").append(options);
                    $(".selected-header").focus();
                    console.log(result.msg);
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

// Save data from excel file
function UploadAsset() {
    LoadingShow();
    var action = baseUrl + 'ReadExcelFile';
    var datasend = JSON.stringify({
        selectedSheet: $(".selected-sheet").val(),
        selectedHeader: $(".selected-header").val()
    });

    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            $('#modalITAssetsUpload').modal("hide");
            if (response.file) {
                toastr.options = {
                    "closeButton": true,
                    "progressBar": true,
                    "timeOut": "10000",
                    "positionClass": "toast-top-center",
                }
                toastr.success('Tải file lỗi <a href="' + response.file + '" download="error_import_file.xlsx">tại đây</a>', 'Thành công');
            }
            else {
                toastr.success(response.msg);
                setTimeout(function () {
                    ITAssetRefresh();
                }, 1500)
            }
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// Backup data
function ITAssetBackup() {
    LoadingShow();
    var action = baseUrl + 'Backup';
    var datasend = JSON.stringify({
    });

    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            setTimeout(function () {
                ITAssetRefresh();
            }, 1500);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// Print countsheet
function Print() {
    LoadingShow();
    var action = baseUrl + 'Print';
    var datasend = {
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
            toastr.error("Chưa có tài sản được kiểm kê. / There are no assets inventoried.")
        }
    })
}

// Add asset
function AddAsset() {
    //CheckNullOrEmpty(tag, "Tag không được để trống /Tag can not be blank");
    //CheckNullOrEmpty(serial, "Serial không được để trống /Serial can not be blank");
    //CheckNullOrEmpty(model, "Model không được để trống /Model can not be blank");
    //CheckNullOrEmpty(user, "User không được để trống /User can not be blank");
    //CheckNullOrEmpty(status, "Status không được để trống /Status can not be blank");

    let asset = {
        data: {
            TAG: tag.val(),
            SERIAL: serial.val(),
            MODEL: model.val(),
            USER: user.val(),
            STATUS: txt_status.val(),
            DEPT: dept.val(),
            DIVISION: division.val(),
            PUR_DATE: purDate.val(),
            WARRANTY: warrantyDate.val(),
            COUNTSHEET: countSheet.val(),
            NOTES: note.val()
        }
    }

    var action = baseUrl + 'AddNewPC';
    var datasend = JSON.stringify(asset);
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            $("#modalITAssetsAddNewPC").modal('hide');
            toastr.success(response.msg);
            lbTotal.text(parseInt(lbTotal.text()) + 1); // change total label

            // append to table
            let data = asset.data;
            let id = response.data.ID;
            let html = "<tr id='it-asset-id-'" + id + " ondblclick='OpenUpdateModal(" + id + ")'>"
                            + "<td>" + id + "</td>"
                            + "<td>" + data.TAG + "</td>"
                            + "<td>" + data.SERIAL + "</td>"
                            + "<td>" + $("#txtModel option:selected").text() + "</td>"
                            + "<td>" + data.PUR_DATE + "</td>"
                            + "<td>" + data.WARRANTY + "</td>"
                            + "<td>" + $("#select-division option:selected").text() + "</td>"
                            + "<td>" + $("#select-department option:selected").text() + "</td>"
                            + "<td>" + data.USER + "</td>"
                            + "<td>" + data.NOTES + "</td>"
                            + "<td>" + data.STATUS + "</td>"
                            + "<td>" + data.COUNTSHEET + "</td>"
                        + "</tr>";

            tableBody.prepend(html);
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// Update asset
function UpdateAsset() {

    //CheckNullOrEmpty(tag, "Tag không được để trống /Tag can not be blank");
    //CheckNullOrEmpty(serial, "Serial không được để trống /Serial can not be blank");
    //CheckNullOrEmpty(model, "Model không được để trống /Model can not be blank");
    //CheckNullOrEmpty(user, "User không được để trống /User can not be blank");
    //CheckNullOrEmpty(status, "Status không được để trống /Status can not be blank");

    let asset = {
        data: {
            ID: asset_id.val(),
            TAG: tag.val(),
            SERIAL: serial.val(),
            MODEL: model.val(),
            USER: user.val(),
            STATUS: txt_status.val(),
            DEPT: dept.val(),
            DIVISION: division.val(),
            PUR_DATE: purDate.val(),
            WARRANTY: warrantyDate.val(),
            COUNTSHEET: countSheet.val(),
            NOTES: note.val()
        }
    }

    var action = baseUrl + 'UpdateAsset';
    var datasend = JSON.stringify(asset);
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            ResetControl();
            if (response.data.ISSCAN === true || response.data.ISSCAN === "true") {
                $('#it-asset-id-' + response.data.ID).css("display", "none");

                lbTotal.text(parseInt(lbTotal.text()) - 1);
            }
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// Open add modal
function OpenAddModal(id) {
    $(".add-pc-title").text("Add asset");
    $("#btnAddAsset").css("display", "inline");
    $("#btnUpdateAsset").css("display", "none");
    $("#modalITAssetsAddNewPC").modal('show');
    tag.focus();
    ResetControl();
}

// Open scan modal
function OpenScanModal() {
    $(".add-pc-title").text("Scan asset");
    $("#btnAddAsset").css("display", "none");
    $("#btnUpdateAsset").css("display", "inline");
    $("#modalITAssetsAddNewPC").modal('show');
    tag.focus();
    ResetControl();
    txt_status.val(moment(new Date()).format("DD/MM/YYYY"));
}

// Open update modal
function OpenUpdateModal(id) {
    var obj = {};

    $(".add-pc-title").text("Update asset");
    $("#btnAddAsset").css("display", "none");
    $("#btnUpdateAsset").css("display", "inline");
    $("#modalITAssetsAddNewPC").modal('show');
    $("#txtTag").focus();

    // get data
    var action = baseUrl + 'GetAssetById';
    var datasend = JSON.stringify({
        data: {
            id: id
        }
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            obj = response.data;
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });

    // asign data
    setTimeout(function () {
        asset_id.val(obj.ID);
        tag.val(obj.TAG);
        serial.val(obj.SERIAL);
        model.select2().val(obj.MODEL).trigger("change");
        division.select2().val(obj.DIVISION).trigger("change");
        dept.select2().val(obj.DEPT).trigger("change");
        user.val(obj.USER);
        note.val(obj.NOTE);
        txt_status.val(obj.STATUS);
        countSheet.val(obj.COUNTSHEET);
        purDate.val(obj.PUR_DATE);
        warrantyDate.val(obj.WARRANTY);
    }, 200)
}

// 

// add new model
function AddNewModel() {
    let model = $("#txtNewModel");
    CheckNullOrEmpty(model, "Tên model không được để trống /Model name can not be blank");
    var action = baseUrl + 'AddNewModel';
    var datasend = JSON.stringify({
        data: {
            NAME: model.val()
        }
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            $("#modalAddModel").modal('hide');
            $("#txtModel").append("<option value='" + response.data.ID + "' selected>" + response.data.NAME + "</option>");
            $('#txtModel').trigger('change');
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// add new division
function AddNewDiv() {
    let division = $("#txtNewDiv");
    CheckNullOrEmpty(division, "Tên division không được để trống /Division name can not be blank");
    var action = baseUrl + 'AddNewDivision';
    var datasend = JSON.stringify({
        data: {
            NAME: division.val()
        }
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            $("#modalAddDiv").modal('hide');
            $("#select-division").append("<option value='" + response.data.ID + "' selected>" + response.data.NAME + "</option>");
            $('#select-division').trigger('change');
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// add new department
function AddNewDept() {
    let dept = $("#txtNewDept");
    CheckNullOrEmpty(dept, "Tên department không được để trống /Department name can not be blank");
    var action = baseUrl + 'AddNewDepartment';
    var datasend = JSON.stringify({
        data: {
            NAME: dept.val()
        }
    });
    LoadingShow();
    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {
            LoadingHide();
            toastr.success(response.msg);
            $("#modalAddDept").modal('hide');
            $("#select-department").append("<option value='" + response.data.DEPT_ID + "' selected>" + response.data.NAME + "</option>");
            $('#select-department').trigger('change');
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}
