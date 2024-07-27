$().ready(function () {
    $('#body-content').hide()
    var id;
    var schedDate;
    var schedStartTime;
    var schedEndTime;
    history.pushState(null, null, location.href);



    $(document).ready(function () {
        $("[id^=edit_]").click(function (event) {
            event.preventDefault();
            var code = $(this).data("course-code");
            $.post("../Home/CourseSearch", {
                code: code
            }, function (data) {
                if (data[0].mess == 0) {
                    $("#update_title").val(data[0].title);
                    $("#update_code").val(data[0].code);
                    $("#update_courseType").val(data[0].course_type);
                    $("#update_units").val(data[0].units);
                    $("#update_time").val(data[0].time);
                    $("#update_block").val(data[0].block);
                    $("#update_description").val(data[0].description);

                    // Clear existing schedule checkboxes
                    $("input[name='schedule']").prop('checked', false);

                    // Set schedule checkboxes based on the data
                    var schedule = data[0].schedule.split(','); // Assuming schedule is a comma-separated string
                    schedule.forEach(function (day) {
                        $("input[name='schedule'][value='" + day.trim().toLowerCase() + "']").prop('checked', true);
                    });
                } else {
                    alert("No Subject Found!");
                }
            });
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

    $("#update").click(function (event) {
        event.preventDefault();

        var title = $("#update_title").val();
        var code = $("#update_code").val();
        var courseType = $("#update_courseType").val();
        var units = $("#update_units").val();
        var time = $("#update_time").val();
        var block = $("#update_block").val();
        var description = $("#update_description").val();

        // Collect the schedule values
        var schedule = [];
        $("input[name='schedule']:checked").each(function () {
            schedule.push($(this).val());
        });

        $.post("../Home/CourseUpdate", {
            title: title,
            code: code,
            courseType: courseType,
            units: units,
            time: time,
            block: block,
            description: description,
            schedule: schedule.join(',') // Join array to a comma-separated string
        }, function (data) {
            if (data[0].mess == 0) {
                alert("The data was successfully updated");
                location.reload();
            }
        });
    });



    $("[id^=delete_]").click(function (event) {
        event.preventDefault();
        var code = $(this).data("course-code");

        $.post("../Home/deleteCourse", {
            code: code

        }, function (data) {
            
            if (data[0].mess == 0) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: 'Course was successfully deleted'
                }).then(function () {
                    location.reload()
                });

            }
            else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Unable to delete course.'
                });
            }
           
        });
    });

 

    $(".denyButton").click(function (event) {
        event.preventDefault();

        const loadingPopup = Swal.fire({
            title: 'Processing...',
            text: 'Please wait while we process your request.',
            allowOutsideClick: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        var formData = new FormData();
        formData.append('student_id', $(this).data('student'))
        formData.append('course_code', $(this).data('code'))
        formData.append('course_section', $(this).data('section'))
        formData.append('contact', $(this).data('contact'))
        formData.append('student_name', $(this).data('name'))

        $.ajax({
            url: '../Home/Deny',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                loadingPopup.close();
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
            },
            error: function (xhr, status, error) {
                loadingPopup.close();
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'An error occurred. Please try again later.'
                });
            }
        });
    });

    $(".approveButton").click(function (event) {
        event.preventDefault();

        const loadingPopup = Swal.fire({
            title: 'Processing...',
            text: 'Please wait while we process your request.',
            allowOutsideClick: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        var formData = new FormData();
        formData.append('student_id', $(this).data('student'))
        formData.append('course_code', $(this).data('code'))
        formData.append('course_section', $(this).data('section'))
        formData.append('contact', $(this).data('contact'))
        formData.append('student_name', $(this).data('name'))

        console.log($(this).data('contact'))

        $.ajax({
            url: '../Home/Enroll',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                loadingPopup.close();
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
            },
            error: function (xhr, status, error) {
                loadingPopup.close();
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'An error occurred. Please try again later.'
                });
            }
        });
    });

    $('.card-dashboard').click(function () {
        $('#selected-date').off()
        $('#note').off()
        $('#note').hide()

        id = $(this).data('id')
        var sched = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT']
        var days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']
        var selectedSched = $(this).data('sched').split(', ')
        var selectedTime = $(this).data('time').split(', ')
        var indexList = []
        indexList = sched.map((day, index) => {
            return selectedSched.includes(day) ? null : index;
        }).filter(index => index !== null);


        $('#dashboard-content').hide()

        $('#selected-course').text($(this).data('code'))
        $('#selected-section').text($(this).data('section'))

        let today = new Date();
        let todayDate = getNextValidDate(today, indexList);
        let day = todayDate.getDay()
        $('#selected-date').val(formatDate(todayDate));
        schedDate = formatDate(todayDate)

        var time = selectedSched.indexOf(sched[day])
        schedStartTime = subtractTime(selectedTime[time].split(' - ')[0])
        schedEndTime = selectedTime[time].split(' - ')[1]
        $('#selected-sched').text(`(${days[day]}, ${selectedTime[time]})`)

        $('#body-content').show()

        let previousDate = formatDate(todayDate);

        getAttendanceList($(this).data('id'), previousDate)
            .then(response => {
                displayAttendance(response);
            })
            .catch(error => {
                console.error(error);
            });

        $('#selected-date').on('input', function () {
            $('#note').hide()

            let date = new Date(this.value);
            let day = date.getDay();

            if (!indexList.includes(day)) {
                console.log('dsdasda')
                previousDate = this.value;
                var time = selectedSched.indexOf(sched[day])
                schedStartTime = subtractTime(selectedTime[time].split(' - ')[0])
                schedEndTime = selectedTime[time].split(' - ')[1]

                $('#selected-sched').text(`(${days[day]}, ${selectedTime[time]})`)

                schedDate = previousDate

                getAttendanceList(id, previousDate)
                    .then(response => {
                        displayAttendance(response);
                    })
                    .catch(error => {
                        // Handle the error here
                        console.error(error);
                    });
            }
            else {
                Swal.fire({
                    icon: 'error',
                    title: 'Alert',
                    text: 'No Schedule.',
                }).then(() => {
                    $('#selected-date').val(previousDate);
                });
            }

       
        });

        $('#note').click(function () {
            var formData = new FormData();

            formData.append('course_id', id);

            $.ajax({
                url: '../Home/Start',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    if (response.success) {
                        window.location.href = "../attendance/attendance?course_id=" + id + "&course_code=" + encodeURIComponent($('#selected-course').text()) + "&course_section=" + encodeURIComponent($('#selected-section').text()) + "&time=" + encodeURIComponent(selectedTime[time].split(' - ')[0]) + "&date=" + encodeURIComponent(schedDate)
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

    $('#back').click(function () {
        $('#dashboard-content').show()
        $('#body-content').hide()
    })

    $('#back-profile').click(function () {
        $('#profile-content').show()
        $('#profile-body-content').hide()
    })

    function formatDate(date) {
        let month = ('0' + (date.getMonth() + 1)).slice(-2);
        let day = ('0' + date.getDate()).slice(-2);
        let year = date.getFullYear();
        return `${year}-${month}-${day}`;
    }

    function isValidDay(date, sched) {
        let day = date.getDay();
        return !sched.includes(day)
    }

    function getNextValidDate(date, sched) {
        while (!isValidDay(date, sched)) {
            date.setDate(date.getDate() - 1);
        }
        return date;
    }

    function getAttendanceList(id, date) {
        return new Promise((resolve, reject) => {
            var formData = new FormData();
            formData.append('course_id', id);
            formData.append('date', date);


            $.ajax({
                url: '../Home/Attendance',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    resolve(response);
                },
                error: function (xhr, status, error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'An error occurred. Please try again later.'
                    });
                    reject(error);
                }
            });
        });
    }

    function displayAttendance(data) {
        var tbody = $('#attendanceTable tbody');
        tbody.empty();

        console.log(data)

        if (data.length > 0) {
            data.forEach(item => {
                let studentId = item["student_id"] ? item["student_id"] : '-';
                let lastName = item["student_lastname"] ? item["student_lastname"] : '-';
                let firstName = item["student_firstname"] ? item["student_firstname"] : '-';
                let middleNameInitial = item["student_midname"] ? item["student_midname"].charAt(0).toUpperCase() + '.' : '-';
                let attendanceTimeIn = item["time_in"] ? item["time_in"].split('.')[0] : '-';
                let attendanceStatus = item["remarks"] ? item["remarks"] : '-';
                let supportingDocs = item["docs"] ? `<a href="../student/GetDocument?fileName=${encodeURIComponent(item["docs"])}" target="_blank">View Document</a>` : '-';

                var row = `
                <tr style="font-size: 12px; font-weight: normal; text-align: center;">
                    <td>${studentId}</td>
                    <td>${lastName}</td>
                    <td>${firstName}</td>
                    <td>${middleNameInitial}</td>
                    <td>${attendanceTimeIn}</td>
                    <td>${attendanceStatus}</td>
                    <td>${supportingDocs}</td>
                </tr>
                `;
                tbody.append(row);
            });
        }
        else {
            const strToday = getCurrentLocalDateTime();
            const strStartSched = `${schedDate}T${schedStartTime}:00`
            const strEndSched = `${schedDate}T${schedEndTime}:00`

            var today = new Date(strToday);
            var startSched = new Date(strStartSched);
            var endSched = new Date(strEndSched);

            if (startSched < today && schedDate == strToday.split('T')[0] && today < endSched) {
                $('#note').show()
            }
            else {
                $('#note').hide()
            }
            
        }
    }

    function getCurrentLocalDateTime() {
        const now = new Date();

        // Extract components
        const year = now.getFullYear();
        const month = String(now.getMonth() + 1).padStart(2, '0'); // Months are zero-based
        const day = String(now.getDate()).padStart(2, '0');
        const hours = String(now.getHours()).padStart(2, '0');
        const minutes = String(now.getMinutes()).padStart(2, '0');
        const seconds = String(now.getSeconds()).padStart(2, '0');

        // Format as YYYY-MM-DDTHH:MM:SS
        return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
    }

    function subtractTime(timeString) {
        // Split time string into hours and minutes
        const [hours, minutes] = timeString.split(':').map(Number);

        // Create a Date object with the given time
        const date = new Date();
        date.setHours(hours);
        date.setMinutes(minutes);
        date.setSeconds(0);

        // Subtract the specified number of minutes
        date.setMinutes(date.getMinutes() - 15);

        // Format the result back to HH:MM
        const newHours = String(date.getHours()).padStart(2, '0');
        const newMinutes = String(date.getMinutes()).padStart(2, '0');

        return `${newHours}:${newMinutes}`;
    }

    $('.card-profile').click(function () {
        $('#profile-selected-course').text($(this).data('code'))
        $('#profile-selected-section').text($(this).data('section'))
        $('#profile-selected-sched').text(`(${$(this).data('sched') }, ${$(this).data('time') })`)
        $('#profile-content').hide()
        $('#profile-body-content').show()
        var url;
        var formData = new FormData()

        
        var formData = new FormData();
        formData.append('course_id', $(this).data('id'))

        $.ajax({
            url: '../Home/Profiles',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                console.log(response)

                $('#profileTable tbody').empty();
                response.forEach(function (student) {
                    var row = `
                    <tr style="font-size: 10px; font-weight: normal; text-align: center;"">
                        <td>${student.Status}</td>
                        <td>${student.ID}</td>
                        <td>${student.Student_Fullname}</td>
                        <td>${student.Student_Contact}</td>
                        <td>${student.Guardian_Fullname}</td>
                        <td>${student.Guardian_Contact}</td>
                        <td>${student.Relationship_to_Guardian}</td>
                        <td>${student.Total_Late}</td>
                        <td>${student.Total_Absent}</td>
                `;

                    if (student.Status === 'ENROLLED') {
                        row += `<td><button class="btn btn-danger profileAction" style="width: 100%; font-size: 10px; font-weight: normal; text-align: center;" data-id="${student.ID}" data-name="${student.Student_Fullname}" data-contact="${student.Student_Contact}" data-url="../Home/Unenroll">Unenroll</button></td>`;
                    }
                    else if (student.Status == 'PENDING') {
                        row += `<td><button class="btn btn-success profileAction" style="width: 100%; font-size: 10px; font-weight: normal; text-align: center;" data-id="${student.ID}" data-name="${student.Student_Fullname}" data-contact="${student.Student_Contact}" data-url="../Home/Enroll">Enroll</button></td>`;
                    }
                    else if (student.Status == 'TO BE DROPPED') {
                        row += `<td><button class="btn btn-warning profileAction" style="width: 100%; font-size: 10px; font-weight: normal; text-align: center;" data-id="${student.ID}" data-url="../Home/deleteDrop">Change</button></td>`;
                    }
                    else {
                        row += '<td></td>';
                    }

                    row += '</tr>';

                    $('#profileTable tbody').append(row);

                    
                });

                $('.profileAction').click(function () {
                    formData.append('student_id', $(this).data('id'))

                    if ($(this).text() != 'TO BE DROPPED') {
                        formData.append('student_name', $(this).data('name'))
                        formData.append('contact', $(this).data('contact'))
                        formData.append('course_code', $('#profile-selected-course').text())
                        formData.append('course_section', $('#profile-selected-section').text())
                    }
                    else {
                        formData.append('course_id', $(this).data('id'))
                    }

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
                        url: $(this).data('url'),
                        type: 'POST',
                        data: formData,
                        contentType: false,
                        processData: false,
                        success: function (response) {
                            loadingPopup.close();
                            if (response.success) {
                                Swal.fire({
                                    icon: 'success',
                                    title: 'Success',
                                    text: "Status successfully changed"
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
                        },
                        error: function (xhr, status, error) {
                            loadingPopup.close();
                            Swal.fire({
                                icon: 'error',
                                title: 'Error',
                                text: 'An error occurred. Please try again later.'
                            });
                        }
                    });
                })

                sortTableBySelectedOption();

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

    function sortTableBySelectedOption() {
        const selectedOption = $('#sortOptions').val();
        const $tbody = $('#profileTable tbody');
        const $rows = $tbody.find('tr').get();

        switch (selectedOption) {
            case 'Name':
                sortByName($rows);
                break;
            case 'Late':
                sortByLate($rows);
                break;
            case 'Absent':
                sortByAbsent($rows);
                break;
            default:
                break;
        }

        $tbody.append($rows);
    }

    function sortByName(rows) {
        rows.sort((a, b) => {
            const nameA = $(a).find('td:eq(2)').text().trim().toLowerCase();
            const nameB = $(b).find('td:eq(2)').text().trim().toLowerCase();
            return nameA.localeCompare(nameB);
        });
    }

    function sortByLate(rows) {
        rows.sort((a, b) => {
            const lateA = parseInt($(a).find('td:eq(7)').text().trim(), 10) || 0;
            const lateB = parseInt($(b).find('td:eq(7)').text().trim(), 10) || 0;
            return lateB - lateA; // Sort by descending order
        });
    }

    function sortByAbsent(rows) {
        rows.sort((a, b) => {
            const absentA = parseInt($(a).find('td:eq(8)').text().trim(), 10) || 0;
            const absentB = parseInt($(b).find('td:eq(8)').text().trim(), 10) || 0;
            return absentB - absentA; // Sort by descending order
        });
    }

    $('#sortOptions').on('change', function () {
        sortTableBySelectedOption();
    });
});