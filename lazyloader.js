(function ($) {

    $.fn.lazyloader = function (options) {
        var settings = {
            threshold: 0,
            failurelimit: 0,
            event: "scroll",
            effect: "show",
            container: window,
            filename: "1x1.gif"
        };

        if (options) {
            $.extend(settings, options);
        }

        /* Fire one scroll event per scroll. Not one scroll event per image. */
        var elements = this;
        var temp = $.grep(elements, function (element) {
            return !element.loaded;
        });
        elements = $(temp);
        elements.each(function () {
            var self = this;

            if (!$(self).loaded) {
                $(self).one("appear", function () {
                    if (!this.loaded) {
                        $(this).hide().attr("src", $(this).attr("originalsrc"))[settings.effect](settings.effectspeed);
                        this.loaded = true;
                    };
                });
            }
        });



        if ("scroll" == settings.event) {
            $(settings.container).bind("scroll", function (event) {

                var counter = 0;
                elements.each(function () {
                    if ($.abovethetop(this, settings) || $.leftofbegin(this, settings) || $.rightoffold(this, settings)) {
                        $(this).trigger("appear");
                    } else if (!$.belowthefold(this, settings)) {
                        $(this).trigger("appear");
                    }
                });
                /* Remove image from array so it is not looped next time. */
                var temp = $.grep(elements, function (element) {
                    return !element.loaded;
                });
                elements = $(temp);
            });
        } else if ("load" == settings.event) {
            $(elements).each(function () {
                var self = this;
                if ($(self).attr("originalsrc") != undefined) {
                    $(self).hide().attr("src", $(self).attr("originalsrc"))[settings.effect](settings.effectspeed);
                    $(self).loaded = true;
                }
            });
        }

        /* Force initial check if images should appear. */
        $(settings.container).trigger(settings.event);

        return this;

    };

    /* Convenience methods in jQuery namespace.           */
    /* Use as  $.belowthefold(element, {threshold : 100, container : window}) */

    $.belowthefold = function (element, settings) {
        if (settings.container === undefined || settings.container === window) {
            var fold = $(window).height() + $(window).scrollTop();
        } else {
            var fold = $(settings.container).offset().top + $(settings.container).height();
        }
        return fold <= $(element).offset().top - settings.threshold;
    };

    $.rightoffold = function (element, settings) {
        if (settings.container === undefined || settings.container === window) {
            var fold = $(window).width() + $(window).scrollLeft();
        } else {
            var fold = $(settings.container).offset().left + $(settings.container).width();
        }
        return fold <= $(element).offset().left - settings.threshold;
    };

    $.abovethetop = function (element, settings) {
        if (settings.container === undefined || settings.container === window) {
            var fold = $(window).scrollTop();
        } else {
            var fold = $(settings.container).offset().top;
        }
        return fold >= $(element).offset().top + settings.threshold + $(element).height();
    };

    $.leftofbegin = function (element, settings) {
        if (settings.container === undefined || settings.container === window) {
            var fold = $(window).scrollLeft();
        } else {
            var fold = $(settings.container).offset().left;
        }
        return fold >= $(element).offset().left + settings.threshold + $(element).width();
    };
    /* Custom selectors for your convenience.   */
    /* Use as $("img:below-the-fold").something() */

    $.extend($.expr[':'], {
        "below-the-fold": "$.belowthefold(a, {threshold : 0, container: window})",
        "above-the-fold": "!$.belowthefold(a, {threshold : 0, container: window})",
        "right-of-fold": "$.rightoffold(a, {threshold : 0, container: window})",
        "left-of-fold": "!$.rightoffold(a, {threshold : 0, container: window})"
    });

})(jQuery);


$(document).ready(function () {
    $("img").lazyloader({
        effect: "fadeIn",
        event: "scroll"
    });
});