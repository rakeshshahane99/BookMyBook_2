$(document).ready(function () {
    LoadDataTable();
});



function LoadDataTable()
{
    datatable = $('#tbldata1').DataTable({
        "ajax": {
            "url": "/Admin/Company/GetAll",
            "type": "GET"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "streetAddress", "width": "15%" },
            { "data": "city", "width": "15%" },
            { "data": "state", "width": "15%" },
            { "data": "postalCode", "width": "15%" },         
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="btn-group" role="group">
                        <a href="/Admin/Company/Upsert?id=${data}"
                            class="btn btn-group btn btn-primary mx-3" role="group">
                            <i class="bi bi-pencil-square">Edit</i>
                        </a>
                        <a onClick=Delete('/Admin/Company/Delete/${data}')
                            class="btn btn-group btn btn btn-danger mx-3" role="group">
                            <i class="bi bi-trash">Delete</i>
                        </a>
                    </div> `
                },
                "width": "15%"
            }

        ]
    });

}

function Delete(url)
{
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type:"Delete",
                success: function (data) {
                    if (data.success) {
                        datatable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })

}
