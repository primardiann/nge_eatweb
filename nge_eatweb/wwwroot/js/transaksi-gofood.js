document.addEventListener("DOMContentLoaded", function () {
    const dropdown = document.getElementById("itemsDropdown");

    fetch("/Items/GetAll") // Endpoint ini harus kamu buat
        .then(res => res.json())
        .then(data => {
            data.forEach(item => {
                const opt = document.createElement("option");
                opt.value = item.id_items;
                opt.textContent = item.nama_item;
                dropdown.appendChild(opt);
            });
        });
});
