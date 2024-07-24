$(document).ready(function () {
    function validateIntegerInput(input) {
        input.value = input.value.replace(/[^0-9]/g, '');
    }

    $('.identificationNumber').on('input', function () {
        validateIntegerInput(this);
    })

    document.getElementById('imageUpload').addEventListener('change', function (event) {
        var reader = new FileReader();
        reader.onload = function () {
            var output = document.getElementById('imagePreview');
            output.style.backgroundImage = 'url(' + reader.result + ')';
        };
        reader.readAsDataURL(event.target.files[0]);
    });

    $("#createForm").submit(function (event) {
        event.preventDefault();
        var formData = new FormData(this);

        $.ajax({
            url: '../auth/Create',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: response.message
                    }).then(function () {
                        window.location.href = '../auth/login';
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'An error occurred. Please try again later.'
                });
            }
        });
    });

    $("#verifyForm").submit(function (event) {
        event.preventDefault();
        var formData = new FormData(this);

        $.ajax({
            url: '../auth/Verify',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    var url;
                    if (response.user == 'Student') {
                        url = '../student/Dashboard'
                    }
                    else {
                        url = '../Home/Admin'
                    }

                    window.location.href = url;

                    
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'An error occurred. Please try again later.'
                });
            }
        });
    });
});