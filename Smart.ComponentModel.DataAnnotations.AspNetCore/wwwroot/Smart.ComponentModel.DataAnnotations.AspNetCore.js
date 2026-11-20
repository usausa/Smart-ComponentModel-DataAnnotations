(function ($) {
    "use strict";

    const numberPattern = /^-?\d+(\.\d+)?$/;
    const datePattern = /^(\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}(:\d{2}(\.\d+)?)?)?|\d{2}:\d{2}(:\d{2}(\.\d+)?)?)$/;

    function getModelPrefix(fieldName) {
        return fieldName.substring(0, fieldName.lastIndexOf(".") + 1);
    }

    function appendModelPrefix(value, prefix) {
        return value.indexOf("*.") === 0 ? value.replace("*.", prefix) : value;
    }

    function escapeAttributeValue(value) {
        return value.replace(/([!"#$%&'()*+,./:;<=>?@\[\\\]^`{|}~])/g, "\\$1");
    }

    function findElements(form, element, name) {
        const fullName = appendModelPrefix(name, getModelPrefix(element.name));
        return $(form).find(":input").filter("[name='" + escapeAttributeValue(fullName) + "']");
    }

    function revalidateOnChange(elements, element) {
        const namespace = ".smartvalidation" + element.name.replace(/[^A-Za-z0-9_]/g, "_");
        elements.off("change" + namespace).on("change" + namespace, function () {
            const validator = $(element.form).data("validator");
            if (validator && ((element.name in validator.submitted) || ($(element).val() !== ""))) {
                $(element).valid();
            }
        });
    }

    function readCondition(elements) {
        const checkbox = elements.filter(":checkbox");
        if (checkbox.length > 0) {
            return checkbox.is(":checked");
        }

        const radio = elements.filter(":radio");
        const value = radio.length > 0 ? radio.filter(":checked").val() : elements.first().val();
        if (typeof value !== "string") {
            return null;
        }

        const text = value.toLowerCase();
        return text === "true" ? true : (text === "false" ? false : null);
    }

    function isConditionMet(params) {
        return readCondition(params.condition) === params.value;
    }

    function hasValue(value, allowEmpty) {
        if (Array.isArray(value)) {
            return value.length > 0;
        }

        if ((typeof value !== "string") || (value.length === 0)) {
            return false;
        }

        return allowEmpty || (value.trim().length > 0);
    }

    function compareValues(value, other, type) {
        if ((typeof value !== "string") || (typeof other !== "string")) {
            return null;
        }

        if (type === "number") {
            if (!numberPattern.test(value) || !numberPattern.test(other)) {
                return null;
            }

            const left = parseFloat(value);
            const right = parseFloat(other);
            return left < right ? -1 : (left > right ? 1 : 0);
        }

        if (type === "date") {
            if ((value.length !== other.length) || !datePattern.test(value) || !datePattern.test(other)) {
                return null;
            }

            return value < other ? -1 : (value > other ? 1 : 0);
        }

        return null;
    }

    function isValidOperation(operation, result) {
        switch (operation) {
            case "LessThan":
                return result < 0;
            case "LessThanOrEqual":
                return result <= 0;
            case "GreaterThan":
                return result > 0;
            case "GreaterThanOrEqual":
                return result >= 0;
            default:
                return true;
        }
    }

    function isComparisonValid(value, params) {
        const result = compareValues(value, params.other.first().val(), params.type);
        return (result === null) || isValidOperation(params.operation, result);
    }

    function ms932Length(value) {
        let length = 0;
        for (let i = 0; i < value.length; i++) {
            const c = value.charCodeAt(i);
            length += (c <= 0x7f) || ((c >= 0xff61) && (c <= 0xff9f)) || ((c >= 0xd800) && (c <= 0xdfff)) ? 1 : 2;
        }

        return length;
    }

    // Conditional
    $.validator.addMethod("requiredwhen", function (value, element, params) {
        return !isConditionMet(params) || hasValue(value, params.allowempty);
    });

    $.validator.unobtrusive.adapters.add("requiredwhen", ["condition", "value", "allowempty"], function (options) {
        const condition = findElements(options.form, options.element, options.params.condition);
        options.rules.requiredwhen = { condition: condition, value: options.params.value === "true", allowempty: options.params.allowempty === "true" };
        options.messages.requiredwhen = options.message;
        revalidateOnChange(condition, options.element);
    });

    $.validator.addMethod("emptywhen", function (value, element, params) {
        return !isConditionMet(params) || !hasValue(value, params.allowempty);
    });

    $.validator.unobtrusive.adapters.add("emptywhen", ["condition", "value", "allowempty"], function (options) {
        const condition = findElements(options.form, options.element, options.params.condition);
        options.rules.emptywhen = { condition: condition, value: options.params.value === "true", allowempty: options.params.allowempty === "true" };
        options.messages.emptywhen = options.message;
        revalidateOnChange(condition, options.element);
    });

    // Comparison
    $.validator.addMethod("compareto", function (value, element, params) {
        return this.optional(element) || isComparisonValid(value, params);
    });

    $.validator.unobtrusive.adapters.add("compareto", ["other", "operation", "type"], function (options) {
        const other = findElements(options.form, options.element, options.params.other);
        options.rules.compareto = { other: other, operation: options.params.operation, type: options.params.type };
        options.messages.compareto = options.message;
        revalidateOnChange(other, options.element);
    });

    $.validator.addMethod("comparetowhen", function (value, element, params) {
        return this.optional(element) || !isConditionMet(params) || isComparisonValid(value, params);
    });

    $.validator.unobtrusive.adapters.add("comparetowhen", ["condition", "value", "other", "operation", "type"], function (options) {
        const condition = findElements(options.form, options.element, options.params.condition);
        const other = findElements(options.form, options.element, options.params.other);
        options.rules.comparetowhen = { condition: condition, value: options.params.value === "true", other: other, operation: options.params.operation, type: options.params.type };
        options.messages.comparetowhen = options.message;
        revalidateOnChange(condition.add(other), options.element);
    });

    // Japanese
    $.validator.addMethod("ms932length", function (value, element, params) {
        if (this.optional(element)) {
            return true;
        }

        const length = ms932Length(value);
        return (length >= params.min) && (length <= params.max);
    });

    $.validator.unobtrusive.adapters.add("ms932length", ["min", "max"], function (options) {
        options.rules.ms932length = { min: parseInt(options.params.min, 10), max: parseInt(options.params.max, 10) };
        options.messages.ms932length = options.message;
    });

    $.validator.addMethod("charactertype", function (value, element, pattern) {
        if (this.optional(element)) {
            return true;
        }

        const match = new RegExp(pattern).exec(value);
        return (match !== null) && (match.index === 0) && (match[0].length === value.length);
    });

    $.validator.unobtrusive.adapters.addSingleVal("charactertype", "pattern");
})(jQuery);
