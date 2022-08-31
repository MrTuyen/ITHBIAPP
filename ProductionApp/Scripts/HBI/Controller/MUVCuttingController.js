/*
    Author: TuyenNV
    Description: RecutController js file
    Date: 20210419
*/
var baseUrl = "/MUVCutting/";

// Action enum
var View_Type = {
    Weekly: 1,
    Daily: 2,
    Style: 3,
    Fabric: 4
}

// General config
$(document).ready(function () {
    // select2
    $("#select-group-filter").select2({
        placeholder: "Select a location",
        allowClear: true
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
    window.location.href = '/MUVCutting/Index';
}

function GoBack() {
    window.history.back();
}

// Untick all checkboxes
function Clear() {
    $("input[type=checkbox]").prop("checked", false);
}

// Display none all table
function HideTable() {
    $("#table1").css("display", "none");
    $("#table2").css("display", "none");
    $("#table3").css("display", "none");
    $("#table4").css("display", "none");

    $("#table1-var").css("display", "none");
    $("#table2-var").css("display", "none");
    $("#table3-var").css("display", "none");
    $("#table4-var").css("display", "none");
}

// Change table
function ViewChange() {
    var view = parseInt($("#txtView").val());
    HideTable();
    switch (view) {
        case View_Type.Weekly:
            {
                $("#table1").css("display", "table");
                $("#table1-var").css("display", "table");
            } break;
        case View_Type.Daily:
            {
                $("#table2").css("display", "table");
                $("#table2-var").css("display", "table");
            } break;
        case View_Type.Style:
            {
                $("#table3").css("display", "table");
                $("#table3-var").css("display", "table");
            } break;
        case View_Type.Fabric:
            {
                $("#table4").css("display", "table");
                $("#table4-var").css("display", "table");
            } break;
    }
}

// Upload file excel
function UploadExcel() {
    var e = event;
    var fileName = e.target.files[0].name;
    $('.fileUploadName').text(fileName);

    if (window.FormData !== undefined) {

        var fileUpload = $("#fileSellingUpload").get(0);
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
function UploadSelling() {
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
                setTimeout(function () {
                    Refresh();
                }, 1500)
            }
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
            setTimeout(function () {
                Refresh();
            }, 2000)
        }
    });
}

// Upload file excel
function UploadExcelTarget() {
    var e = event;
    var fileName = e.target.files[0].name;
    $('.fileUploadName').text(fileName);

    if (window.FormData !== undefined) {

        var fileUpload = $("#fileTargetUpload").get(0);
        var files = fileUpload.files;

        // Create FormData object
        var fileData = new FormData();

        // Looping over all files and add it to FormData object
        for (var i = 0; i < files.length; i++) {
            fileData.append(files[i].name, files[i]);
        }

        $.ajax({
            url: baseUrl + 'UploadFileTarget',
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

                    $(".selected-sheet-target").html("").append(options);
                    $(".selected-header-target").focus();
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
function UploadTarget() {
    LoadingShow();
    var action = baseUrl + 'ReadExcelFileTarget';
    var datasend = JSON.stringify({
        selectedSheet: $(".selected-sheet-target").val(),
        selectedHeader: $(".selected-header-target").val()
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
                setTimeout(function () {
                    Refresh();
                }, 1500)
            }
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
            setTimeout(function () {
                Refresh();
            }, 2000)
        }
    });
}

// Submit
function Submit() {

    var viewType = $("#txtView").val();
    var plant = $("#txtPlant").val();
    var date = $("#txtFromDate").val();
    var listWC = [];

    $("input[name='wc']:checked").each(function (index, obj) {
        listWC.push(obj.value);
    })

    if (date <= 0) {
        toastr.error("Vui lòng chọn ngày/ Please choose a date");
        $("#txtFromDate").focus();
        return false;
    }

    if (listWC.length <= 0) {
        toastr.error("Vui lòng chọn ít nhất 1 WC/ Please choose at least one WC");
        return false;
    }

    LoadingShow();
    var action = baseUrl + 'Submit';
    var datasend = JSON.stringify({
        plant: plant,
        date: date,
        wc: listWC,
        viewType: viewType
    });

    PostDataAjax(action, datasend, function (response) {
        if (response.rs) {

            var weekNumber = new Date(date).getWeek();

            switch (parseInt(viewType)) {
                case View_Type.Weekly: {
                    Weekly(response);
                    $("#table1-week").text("Week " + weekNumber);
                } break;
                case View_Type.Daily: {
                    $("#table2-date").text(date);
                    $("#table2-week").text("W" + weekNumber);
                    Daily(response);
                } break;
                case View_Type.Style: {
                    $("#table3-date").text(date);
                    $("#table3-week").text("W" + weekNumber);
                    Style(response);
                } break;
                case View_Type.Fabric: {
                    $("#table4-date").text(date);
                    $("#table4-week").text("W" + weekNumber);
                    Fabric(response);
                } break;
            }

            LoadingHide();
        }
        else {
            LoadingHide();
            toastr.error(response.msg);
        }
    });
}

// 
function GetSymbol(val) {
    var symbol = (val == null || val == 0) ? "<div class='triangle-up'></div>" : (parseFloat(val) > 0 ? "<div class='diamond-narrow'></div>" : "<div class='circle'></div>")
    return symbol;
}
// 
function GetSymbolForSpan(val) {
    var symbol = (val == null || val == 0) ? "<span class='triangle-up'></span>" : (parseFloat(val) > 0 ? "<span class='diamond-narrow'></span>" : "<span class='circle'></span>")
    return symbol;
}
// 
function GetEqual(val) {
    return (val == null || val == 0) ? "-" : parseFloat(val);
}

// Weekly 
function Weekly(response) {
    var data = response.data;
    var sum = response.sum;
    var goal = response.goal;
    var html = "";

    html += "<tr>"
            + "<td></td>"
            + "<td></td>"
            + "<td class='sum'><strong>SUM:</strong>" + sum + "</td>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "</tr>";

    for (var i = 0; i < data.length; i++) {
        var item = data[i];
        var muv = item.Value;

        var symbol1 = GetSymbol(muv[0].Amount);
        var symbol2 = GetSymbol(muv[1].Amount);

        html += "<tr>"
            + "<td>" + item.Key + "</td>"
            + "<td>" + GetEqual(muv[0].Dz) + "</td>"
            + "<td class='symbol'>" + symbol1 + "&nbsp; <span class='ml-5'>" + GetEqual(muv[0].Amount) + "</span></td>"
            + "<td>" + GetEqual(muv[0].RunRate) + "</td>"
            + "<td>" + GetEqual(muv[1].Dz) + "</td>"
            + "<td class='symbol'>" + symbol2 + "&nbsp; <span class='ml-5'>" + GetEqual(muv[1].Amount) + "</span></td>"
            + "<td>" + GetEqual(muv[1].RunRate) + "</td>"
            + "</tr>";
    }

    $("#table1-body").html('');
    $("#table1-body").html(html);

    // Var
    var opcVar = parseFloat(response.sum) - parseFloat(goal.OPC);
    var stretchVar = parseFloat(response.sum) - parseFloat(goal.Stretch);
    html = "<tr>"
            + "<td>" + goal.OPC + "</td>"
            + "<td class='symbol'>" + GetSymbol(opcVar) + "&nbsp; <span class='ml-5'>" + GetEqual(opcVar) + "</span></td>"
            + "<td>" + goal.Stretch + "</td>"
            + "<td class='symbol'>" + GetSymbol(stretchVar) + "&nbsp; <span class='ml-5'>" + GetEqual(stretchVar) + "</span></td>"
            + "</tr>";

    $("#table1-var-body").html('');
    $("#table1-var-body").html(html);
}

// Daily 
function Daily(response) {
    var data = response.data;
    var sum = response.sum;
    var goal = response.goal;
    var html = "";

    html += "<tr>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td class='sum'><strong>SUM:</strong>" + sum + "</td>"
            + "<td></td>"
            + "</tr>";

    for (var i = 0; i < data.length; i++) {
        var item = data[i];
        var muv = item.Value;

        var symbol1 = GetSymbol(muv[0].Amount);
        var symbol2 = GetSymbol(muv[1].Amount);

        html += "<tr>"
            + "<td>" + item.Key + "</td>"
            + "<td>" + GetEqual(muv[0].Dz) + "</td>"
            + "<td class='symbol'>" + symbol1 + "&nbsp; <span class='ml-5'>" + GetEqual(muv[0].Amount) + "</span></td>"
            + "<td>" + GetEqual(muv[0].RunRate) + "</td>"
            + "<td>" + GetEqual(muv[1].Dz) + "</td>"
            + "<td class='symbol'>" + symbol2 + "&nbsp; <span class='ml-5'>" + GetEqual(muv[1].Amount) + "</span></td>"
            + "<td>" + GetEqual(muv[1].RunRate) + "</td>"
            + "</tr>";
    }

    $("#table2-body").html('');
    $("#table2-body").html(html);

    // Var
    var opcVar = parseFloat(response.sum) - parseFloat(goal.OPC);
    var stretchVar = parseFloat(response.sum) - parseFloat(goal.Stretch);
    html = "<tr>"
            + "<td>" + goal.OPC + "</td>"
            + "<td class='symbol'>" + GetSymbol(opcVar) + "&nbsp; <span class='ml-5'>" + GetEqual(opcVar) + "</span></td>"
            + "<td>" + goal.Stretch + "</td>"
            + "<td class='symbol'>" + GetSymbol(stretchVar) + "&nbsp; <span class='ml-5'>" + GetEqual(stretchVar) + "</span></td>"
            + "</tr>";

    $("#table2-var-body").html('');
    $("#table2-var-body").html(html);
}

// Style 
function Style(response) {
    var data = response.data;
    var sum = response.sum;
    var goal = response.goal;

    var totalSum = 0;
    sum.forEach(function (ele) {
        totalSum += ele.Value;
    })
    var html = "";
    html += "<tr>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td class='sum'><strong>SUM:</strong>" + totalSum + "</td>"
            + "<td></td>"
            + "</tr>";
    for (var i = 0; i < data.length; i++) {
        var item = data[i];

        for (var k = 0; k < item.Value.day.length; k++) {
            var styleDay = item.Value.day[k];
            var styleWeek = item.Value.week.filter(function (ele) {
                return ele.Style == styleDay.Style;
            })[0];

            const index = item.Value.week.indexOf(styleWeek);
            if (index > -1) {
                item.Value.week.splice(index, 1);
            }

            var symbol1 = GetSymbol(styleDay.Amount);
            var symbol2 = GetSymbol(styleWeek.Amount);

            html += "<tr>"
                + "<td>" + item.Key + "</td>"
                + "<td>" + styleDay.Style + "</td>"
                + "<td>" + GetEqual(styleDay.Dz) + "</td>"
                + "<td class='symbol'>" + symbol1 + "&nbsp; <span class='ml-5'>" + GetEqual(styleDay.Amount) + "</span></td>"
                + "<td>" + GetEqual(styleDay.RunRate) + "</td>"
                + "<td>" + GetEqual(styleWeek.Dz) + "</td>"
                + "<td class='symbol'>" + symbol2 + "&nbsp; <span class='ml-5'>" + GetEqual(styleWeek.Amount) + "</span></td>"
                + "<td>" + GetEqual(styleWeek.RunRate) + "</td>"
                + "</tr>";
        }


        for (var j = 0; j < item.Value.week.length; j++) {
            var styleWeek = item.Value.week[j];

            var symbol1 = GetSymbol(0);
            var symbol2 = GetSymbol(styleWeek.Amount);

            html += "<tr>"
                  + "<td>" + item.Key + "</td>"
                  + "<td>" + styleWeek.Style + "</td>"
                  + "<td></td>"
                  + "<td></td>"
                  + "<td></td>"
                  + "<td>" + GetEqual(styleWeek.Dz) + "</td>"
                  + "<td class='symbol'>" + symbol2 + "&nbsp; <span class='ml-5'>" + GetEqual(styleWeek.Amount) + "</span></td>"
                  + "<td>" + GetEqual(styleWeek.RunRate) + "</td>"
                  + "</tr>";
        }
    }

    $("#table3-body").html('');
    $("#table3-body").html(html);

    // Var
    html = "";
    var opc = 0;
    var stretch = 0;
    goal.forEach(function (ele) {
        opc += (ele.Value.length > 0 ? ele.Value[0].OPC : 0);
        stretch += (ele.Value.length > 0 ? ele.Value[0].Stretch : 0);
    });

    var opcVar = totalSum - opc;
    var stretchVar = totalSum - stretch;
    html += "<tr>"
            + "<td>" + opc + "</td>"
            + "<td class='symbol'>" + GetSymbol(opcVar) + "&nbsp; <span class='ml-5'>" + GetEqual(opcVar) + "</span></td>"
            + "<td>" + stretch + "</td>"
            + "<td class='symbol'>" + GetSymbol(stretchVar) + "&nbsp; <span class='ml-5'>" + GetEqual(stretchVar) + "</span></td>"
            + "</tr>";

    $("#table3-var-body").html('');
    $("#table3-var-body").html(html);
}

// Fabric 

function Fabric(response) {
    var data = response.data;
    var sum = response.sum;
    var goal = response.goal;

    var totalSum = 0;
    sum.forEach(function (ele) {
        totalSum += ele.Value;
    })

    var html = "";
    html += "<tr>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td></td>"
            + "<td class='sum'><strong>SUM:</strong>" + totalSum + "</td>"
            + "<td></td>"
            + "</tr>";

    for (var i = 0; i < data.length; i++) {
        var item = data[i];

        //var pickDay = Object.keys(item.Value[0]).map((key) => [String(key), item.Value[0][key]]);

        for (var k = 0; k < item.Value.day.length; k++) {
            var styleDay = item.Value.day[k];
            var styleWeek = item.Value.week.filter(function (ele) {
                return ele.Fabric == styleDay.Fabric;
            })[0];

            const index = item.Value.week.indexOf(styleWeek);
            if (index > -1) {
                item.Value.week.splice(index, 1);
            }

            var symbol1 = GetSymbol(styleDay.Amount);
            var symbol2 = GetSymbol(styleWeek.Amount);

            html += "<tr>"
                + "<td>" + item.Key + "</td>"
                + "<td>" + styleDay.Fabric + "</td>"
                + "<td>" + GetEqual(styleDay.Dz) + "</td>"
                + "<td class='symbol'>" + symbol1 + "&nbsp; <span class='ml-5'>" + GetEqual(styleDay.Amount) + "</span></td>"
                + "<td>" + GetEqual(styleDay.RunRate) + "</td>"
                + "<td>" + GetEqual(styleWeek.Dz) + "</td>"
                + "<td class='symbol'>" + symbol2 + "&nbsp; <span class='ml-5'>" + GetEqual(styleWeek.Amount) + "</span></td>"
                + "<td>" + GetEqual(styleWeek.RunRate) + "</td>"
                + "</tr>";
        }


        for (var j = 0; j < item.Value.week.length; j++) {
            var styleWeek = item.Value.week[j];

            var symbol1 = GetSymbol(0);
            var symbol2 = GetSymbol(styleWeek.Amount);

            html += "<tr>"
                  + "<td>" + item.Key + "</td>"
                  + "<td>" + styleWeek.Fabric + "</td>"
                  + "<td></td>"
                  + "<td></td>"
                  + "<td></td>"
                  + "<td>" + GetEqual(styleWeek.Dz) + "</td>"
                  + "<td class='symbol'>" + symbol2 + "&nbsp; <span class='ml-5'>" + GetEqual(styleWeek.Amount) + "</span></td>"
                  + "<td>" + GetEqual(styleWeek.RunRate) + "</td>"
                  + "</tr>";
        }
    }

    $("#table4-body").html('');
    $("#table4-body").html(html);

    // Var
    html = "";
    var opc = 0;
    var stretch = 0;
    goal.forEach(function (ele) {
        opc += (ele.Value.length > 0 ? ele.Value[0].OPC : 0);
        stretch += (ele.Value.length > 0 ? ele.Value[0].Stretch : 0);
    });

    var opcVar = totalSum - opc;
    var stretchVar = totalSum - stretch;
    html += "<tr>"
            + "<td>" + opc + "</td>"
            + "<td class='symbol'>" + GetSymbol(opcVar) + "&nbsp; <span class='ml-5'>" + GetEqual(opcVar) + "</span></td>"
            + "<td>" + stretch + "</td>"
            + "<td class='symbol'>" + GetSymbol(stretchVar) + "&nbsp; <span class='ml-5'>" + GetEqual(stretchVar) + "</span></td>"
            + "</tr>";

    $("#table4-var-body").html('');
    $("#table4-var-body").html(html);
}

// Download report
function Report() {
    var viewType = $("#txtView").val();
    LoadingShow();
    var action = baseUrl + 'Report';
    var datasend = {
        viewType: viewType,
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
            var result;
            var regex = /{(.*?)}/g;
            var temp = response.match(regex);
            if (temp == null || temp.length < 0) {
                result = "Bạn chưa submit để lấy kết quả trả ra báo cáo/ You has not submit to save result";
            }
            else {
                result = JSON.parse(temp[0]).msg;
            }
            LoadingHide();
            toastr.error(result);
        }
    })
}
