function LoadingShow() {
    $('.full-overlay').css({ 'z-index': 1000000, 'opacity': .5 });
    $('#mainLoadingSVG').show();
}

function LoadingHide() {
    $('.full-overlay').css({ 'z-index': -1, 'opacity': 0 });
    $('#mainLoadingSVG').hide();
}

function CheckNullOrEmpty(input, strError) {
    if (input === undefined || input.val() == null || input.val().trim() === "" || input.val().trim() == "") {
        toastr.error(strError);
        input.focus();
        return false;
    }
    return true;
}

function compareDates(valueTuThangNam, valueDenThangNam, messageName) {
    var oReturn = {
        messErrorForCustomer: '',
        messErrorForCoder: '',
        result: true
    };

    var arrDenThangNam = valueDenThangNam.split('/');
    var arrTuThangNam = valueTuThangNam.split('/');
    var DenThangNam = new Date(arrDenThangNam[2] + '-' + arrDenThangNam[1] + '-' + arrDenThangNam[0]);
    var TuThangNam = new Date(arrTuThangNam[2] + '-' + arrTuThangNam[1] + '-' + arrTuThangNam[0]);
    var dateTime1 = new Date(TuThangNam).getTime(),
        dateTime2 = new Date(DenThangNam).getTime();
    var diff = dateTime2 - dateTime1;
    if (diff < 0) {
        oReturn.result = false;
        oReturn.messErrorForCustomer = messageName;
        oReturn.messErrorForCoder = messageName;
    }
    return oReturn;
}

Date.prototype.getWeek = function (dowOffset) {
    /*getWeek() was developed by Nick Baicoianu at MeanFreePath: http://www.meanfreepath.com */

    dowOffset = typeof (dowOffset) == 'int' ? dowOffset : 0; //default dowOffset to zero
    var newYear = new Date(this.getFullYear(), 0, 1);
    var day = newYear.getDay() - dowOffset; //the day of week the year begins on
    day = (day >= 0 ? day : day + 7);
    var daynum = Math.floor((this.getTime() - newYear.getTime() -
    (this.getTimezoneOffset() - newYear.getTimezoneOffset()) * 60000) / 86400000) + 1;
    var weeknum;
    //if the year starts before the middle of a week
    if (day < 4) {
        weeknum = Math.floor((daynum + day - 1) / 7) + 1;
        if (weeknum > 52) {
            nYear = new Date(this.getFullYear() + 1, 0, 1);
            nday = nYear.getDay() - dowOffset;
            nday = nday >= 0 ? nday : nday + 7;
            /*if the next year starts before the middle of
              the week, it is week #1 of that year*/
            weeknum = nday < 4 ? 1 : 53;
        }
    }
    else {
        weeknum = Math.floor((daynum + day - 1) / 7);
    }
    return weeknum;
};