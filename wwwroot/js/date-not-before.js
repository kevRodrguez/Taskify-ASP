(function ($) {
    if (!$ || !$.validator) {
        return;
    }

    $.validator.addMethod("datenotbefore", function (value, element, params) {
        if (!value) {
            return true;
        }

        var other = $(params).val();
        if (!other) {
            return true;
        }

        return value >= other;
    });

    $.validator.unobtrusive.adapters.add("datenotbefore", ["other"], function (options) {
        var prefix = options.element.name.lastIndexOf(".") >= 0
            ? options.element.name.substring(0, options.element.name.lastIndexOf(".") + 1)
            : "";
        var other = options.params.other;
        var fullOther = other.indexOf(".") >= 0 ? other : prefix + other;

        options.rules.datenotbefore = "[name='" + fullOther + "']";
        options.messages.datenotbefore = options.message;
    });
})(window.jQuery);
