$(document).ready(function () {

    $('#forgotpass').click(function () {
        var formData = new FormData()
        formData.append('stud_id', $('#stud_id').val())

        const loadingPopup = Swal.fire({
            title: 'Processing...',
            text: 'Please wait while we process your request.',
            allowOutsideClick: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        $.ajax({
            url: '../auth/forgotpass',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                loadingPopup.close()
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
                loadingPopup.close()
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'An error occurred. Please try again later.'
                });
            }
        });
    })

    function validateIntegerInput(input) {
        input.value = input.value.replace(/[^0-9]/g, '');
    }

    $('.identificationNumber').on('input', function () {
        validateIntegerInput(this);
    })


    //REGISTER
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

    //LOGIN
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