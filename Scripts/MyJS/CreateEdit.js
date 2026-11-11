$(document).ready(function () {
    if ($.validator) {
        $.extend($.validator.messages, {
            required: "Không được để trống ô này.",
            min: $.validator.format("Vui lòng nhập giá trị lớn hơn hoặc bằng {0}."),
            max: $.validator.format("Vui lòng nhập giá trị nhỏ hơn hoặc bằng {0}."),
            number: "Vui lòng nhập số hợp lệ."
        });
    }
});