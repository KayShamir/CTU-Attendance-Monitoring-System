$().ready(function () {
    let params = new URLSearchParams(window.location.search);

    var time = params.get('time')
    var date = params.get('date')

    let targetDateStr = date + " " + time
    let targetDate = new Date(targetDateStr.replace(' ', 'T'));

    function checkTimeDifference() {
        let now = new Date();

        let timeDifference = now - targetDate;

        let absentTime = timeDifference >= 30 * 60 * 1000;
        let lateTime = timeDifference >= 15 * 60 * 1000;

        console.log("Current Date and Time: " + now);
        console.log("Target Date and Time: " + targetDate);
        console.log("Time Difference (ms): " + timeDifference);
        console.log("Is Within 15 Minutes: " + lateTime);

        if (lateTime && !absentTime) {
            var formData = new FormData()
            formData.append('course_id', params.get("course_id"))

            $.ajax({
                url: '../attendance/Late',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    console.log(response.message)
                },
                error: function (xhr, status, error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'An error occurred. Please try again later.'
                    });
                }
            });
        }
        if (absentTime) {
            var formData = new FormData()
            formData.append('course_id', params.get("course_id"))
            $.ajax({
                url: '../attendance/Absent',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    if (response.success) {
                        window.location.href = '../auth/login'
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
        }
    }

    checkTimeDifference();

    setInterval(checkTimeDifference, 60000);

    $('#record').click(function () {
        var id = $('#idInput').val()

        if (id == "") {
            return
        }
        var formData = new FormData();
        formData.append('student_id', id);
        formData.append('course_id', params.get('course_id'))

        $.ajax({
            url: '../attendance/TimeIn',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Success',
                            text: response.message
                        }).then(function () {
                           location.reload()
                        });

                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: response.message
                        });
                    }
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
    })
})