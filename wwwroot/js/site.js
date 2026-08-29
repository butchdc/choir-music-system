// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {

    document.querySelectorAll("table").forEach(table => {

        const headers = table.querySelectorAll("thead th");

        headers.forEach((header, columnIndex) => {

            const text = header.textContent.trim().toLowerCase();

            // Skip action / empty columns
            if (
                text === "" ||
                text === "actions"
            ) {
                return;
            }

            header.classList.add("sortable-header");

            const icon = document.createElement("i");
            icon.className = "bi bi-arrow-down-up sort-icon ms-1";

            header.appendChild(icon);

            header.addEventListener("click", function () {

                const tbody = table.querySelector("tbody");

                if (!tbody) {
                    return;
                }

                const rows = Array.from(tbody.querySelectorAll("tr"))
                    .filter(row => row.style.display !== "none");

                const currentDirection =
                    header.dataset.sortDirection || "none";

                const direction =
                    currentDirection === "asc"
                        ? "desc"
                        : "asc";

                headers.forEach(h => {
                    h.dataset.sortDirection = "";

                    const i = h.querySelector(".sort-icon");

                    if (i) {
                        i.className =
                            "bi bi-arrow-down-up sort-icon ms-1";
                    }
                });

                header.dataset.sortDirection = direction;

                icon.className =
                    direction === "asc"
                        ? "bi bi-arrow-up sort-icon ms-1"
                        : "bi bi-arrow-down sort-icon ms-1";

                rows.sort((a, b) => {

                    const aCell =
                        a.children[columnIndex];

                    const bCell =
                        b.children[columnIndex];

                    if (!aCell || !bCell) {
                        return 0;
                    }

                    const aValue =
                        aCell.textContent.trim();

                    const bValue =
                        bCell.textContent.trim();

                    const comparison =
                        compareValues(aValue, bValue);

                    return direction === "asc"
                        ? comparison
                        : -comparison;
                });

                rows.forEach(row =>
                    tbody.appendChild(row)
                );

            });

        });

    });


    function compareValues(a, b) {

        const aDate = Date.parse(a);
        const bDate = Date.parse(b);

        if (
            !Number.isNaN(aDate) &&
            !Number.isNaN(bDate) &&
            looksLikeDate(a) &&
            looksLikeDate(b)
        ) {
            return aDate - bDate;
        }

        const aNumber =
            Number(a.replace(/[^0-9.-]/g, ""));

        const bNumber =
            Number(b.replace(/[^0-9.-]/g, ""));

        if (
            a !== "" &&
            b !== "" &&
            !Number.isNaN(aNumber) &&
            !Number.isNaN(bNumber)
        ) {
            return aNumber - bNumber;
        }

        return a.localeCompare(
            b,
            undefined,
            {
                numeric: true,
                sensitivity: "base"
            }
        );
    }


    function looksLikeDate(value) {
        return (
            /\d{1,2}\s+[A-Za-z]{3}\s+\d{4}/.test(value) ||
            /^\d{4}-\d{2}-\d{2}$/.test(value)
        );
    }

});
