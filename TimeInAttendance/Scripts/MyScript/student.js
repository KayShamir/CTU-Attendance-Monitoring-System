$().ready(function () {
    history.pushState(null, null, location.href);

    $("#enrollButton").click(function (event) {
        event.preventDefault();
        var course_code = $('#modalCourseCode').text()
        var course_section = $('#modalSection').text()


        var formData = new FormData();
        formData.append('course_code', course_code)
        formData.append('course_section', course_section)

        $.ajax({
            url: '../student/Enroll',
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
                        window.location.href = '../student/dashboard';
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

    $("#unenrollButton").click(function (event) {
        event.preventDefault();
        var course_code = $('#modalCourseCode').text()
        var course_section = $('#modalSection').text()


        var formData = new FormData();
        formData.append('course_code', course_code)
        formData.append('course_section', course_section)

        $.ajax({
            url: '../student/Unenroll',
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
                        window.location.href = '../student/dashboard';
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

    $('#logoutButton').click(function () {
        Swal.fire({
            title: 'Are you sure?',
            text: 'Do you really want to log out?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, log out',
            cancelButtonText: 'Cancel',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = '../auth/login';
            }
        });
    })
})