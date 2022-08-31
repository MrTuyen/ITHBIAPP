$(document).ready(function () {
    $('.table-active-select').on('click',
        'tr',
        function (event) {
            if ($(this).hasClass('active')) {
                $(this).removeClass('active');
            } else {
                $(this).addClass('active');
            }
        });
    $.ajaxSetup({ cache: false });
    $('form').preventDoubleSubmission();
});
jQuery.fn.preventDoubleSubmission = function () {
    $(this).on('submit', function (e) {
        var $form = $(this);

        if ($form.data('submitted') === true) {
            // Previously submitted - don't submit again
            e.preventDefault();
        } else {
            // Mark it so that the next submit can be ignored
            $form.data('submitted', true);
        }
    });

    // Keep chainability
    return this;
};

//http://gsri1987.github.io/notific8-responsive/
//da trời   tím         đỏ      cam         vàng    xanh lá     đen     bạc
//teal      amethyst    ruby    tangerine   lemon   lime        ebony   smoke
//trash-bin,pencil,lifebuoy,info-circled,fontawesome-webfont,barcode,book,calendar,briefcase-case-two,camera-retro,chat-bubble-two,check-mark-2,cloud,cloud-download,cloud-upload,code,code-fork,cog-gear,css3,cut-scissor,dropbox,facebook-square,file-document,files,filmstrip,flag,folder2,globe-world,google-plus,group,heart,instagram,html5,like-filled,linkedin-square,link,log-in,log-out,male,pinterest-square,photo,qrcode,power-off,random,repeat-redo,reply-all,reply-mail,retweet,rss,save-disk,science-laboratory,stackoverflow,skype,star,star-half,star-half-1,star-two,tachometer,tag-2,ticket,talk-chat-2,talk-chat,tumblr-square,twitter-square,umbrella,user,video-camera,volume-down,volume-down-1,volume-up,window-list,windows,youtube-square,linux,android,apple,bar-chart,bitbucket,box-inbox,bullhorn,bug,compass,credit-card,cutlery,dollar,euro,bitcoin,exclamation-triangle,external-link,,eye,eye-slash-close,facebook,file-text,filter,folder-open,google-plus-1,github-square,github,gears-setting,gamepad,harddrive,home,leaf,location-arrow,magic-wand,mail-forward,mic,mic-no,moon-sleep,minus-circle,paper-clip,pin-map,pin-map-2,pinterest,plane-airport,pound,present-gift,ptint,refresh,road,rss-two,rupee,shield,sitemap,smile,smiley-frown,smiley-meh,tablet,tag,tags,tasks,thumbnails,thumbnails-large,undo,tumblr,twitter,tint,won,wrench,yen,youtube,youtube-play,unlock,unlock-2,amazon,app-store,basecamp,blogger,evernote,dropbox-1,digg,pandora,reddit,steam,stumbleupon,vimeo,ie,chrome,firefox,safari,

function PushAlert(heading, text, theme) {
    if (text != null && text !== "")
        alert(text);
    //var params = {s
    //    life: 5000,

    //    theme: theme,
    //    sticky: false,
    //    horizontalEdge: 'bottom',
    //    verticalEdge: 'right'
    //},

    // icon = 'globe-world';

    //if ($.trim(heading) !== '') {
    //    params.heading = heading;
    //}
    //if ($.trim(icon) !== '') {
    //    params.icon = icon;
    //}

    //// show notification
    //$.notific8(text, params);
}