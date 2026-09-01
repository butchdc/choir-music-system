// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// document.addEventListener("DOMContentLoaded", function () {

//     document.querySelectorAll("table").forEach(table => {

//         const headers = table.querySelectorAll("thead th");

//         headers.forEach((header, columnIndex) => {

//             const text = header.textContent.trim().toLowerCase();

//             // Skip action / empty columns
//             if (
//                 text === "" ||
//                 text === "actions"
//             ) {
//                 return;
//             }

//             header.classList.add("sortable-header");

//             const icon = document.createElement("i");
//             icon.className = "bi bi-arrow-down-up sort-icon ms-1";

//             header.appendChild(icon);

//             header.addEventListener("click", function () {

//                 const tbody = table.querySelector("tbody");

//                 if (!tbody) {
//                     return;
//                 }

//                 const rows = Array.from(tbody.querySelectorAll("tr"))
//                     .filter(row => row.style.display !== "none");

//                 const currentDirection =
//                     header.dataset.sortDirection || "none";

//                 const direction =
//                     currentDirection === "asc"
//                         ? "desc"
//                         : "asc";

//                 headers.forEach(h => {
//                     h.dataset.sortDirection = "";

//                     const i = h.querySelector(".sort-icon");

//                     if (i) {
//                         i.className =
//                             "bi bi-arrow-down-up sort-icon ms-1";
//                     }
//                 });

//                 header.dataset.sortDirection = direction;

//                 icon.className =
//                     direction === "asc"
//                         ? "bi bi-arrow-up sort-icon ms-1"
//                         : "bi bi-arrow-down sort-icon ms-1";

//                 rows.sort((a, b) => {

//                     const aCell =
//                         a.children[columnIndex];

//                     const bCell =
//                         b.children[columnIndex];

//                     if (!aCell || !bCell) {
//                         return 0;
//                     }

//                     const aValue =
//                         aCell.textContent.trim();

//                     const bValue =
//                         bCell.textContent.trim();

//                     const comparison =
//                         compareValues(aValue, bValue);

//                     return direction === "asc"
//                         ? comparison
//                         : -comparison;
//                 });

//                 rows.forEach(row =>
//                     tbody.appendChild(row)
//                 );

//             });

//         });

//     });


//     function compareValues(a, b) {

//         const aDate = Date.parse(a);
//         const bDate = Date.parse(b);

//         if (
//             !Number.isNaN(aDate) &&
//             !Number.isNaN(bDate) &&
//             looksLikeDate(a) &&
//             looksLikeDate(b)
//         ) {
//             return aDate - bDate;
//         }

//         const aNumber =
//             Number(a.replace(/[^0-9.-]/g, ""));

//         const bNumber =
//             Number(b.replace(/[^0-9.-]/g, ""));

//         if (
//             a !== "" &&
//             b !== "" &&
//             !Number.isNaN(aNumber) &&
//             !Number.isNaN(bNumber)
//         ) {
//             return aNumber - bNumber;
//         }

//         return a.localeCompare(
//             b,
//             undefined,
//             {
//                 numeric: true,
//                 sensitivity: "base"
//             }
//         );
//     }


//     function looksLikeDate(value) {
//         return (
//             /\d{1,2}\s+[A-Za-z]{3}\s+\d{4}/.test(value) ||
//             /^\d{4}-\d{2}-\d{2}$/.test(value)
//         );
//     }

// });

/* =========================================================
   GLOBAL CLIENT-SIDE TABLE SORTING
   ========================================================= */

document.addEventListener("DOMContentLoaded", function () {

    document
        .querySelectorAll("table.js-sortable-table")
        .forEach(initializeSortableTable);


    function initializeSortableTable(table) {

        const tbody =
            table.tBodies[0];

        if (!tbody) {
            return;
        }


        const headers =
            Array.from(
                table.querySelectorAll(
                    "thead th[data-sort]"
                )
            );


        headers.forEach(function (header) {

            /*
             * Initial state
             */
            header.dataset.sortDirection = "none";

            header.classList.add(
                "sortable-header"
            );

            header.setAttribute(
                "role",
                "button"
            );

            header.setAttribute(
                "tabindex",
                "0"
            );

            header.setAttribute(
                "aria-sort",
                "none"
            );


            /*
             * Create icon once
             */
            let icon =
                header.querySelector(
                    ".sort-icon"
                );

            if (!icon) {

                icon =
                    document.createElement("i");

                icon.className =
                    "bi bi-chevron-expand sort-icon";

                header.appendChild(icon);

            }


            /*
             * Mouse
             */
            header.addEventListener(
                "click",
                function (event) {

                    event.preventDefault();

                    sortTable(
                        table,
                        header,
                        headers
                    );

                }
            );


            /*
             * Keyboard
             */
            header.addEventListener(
                "keydown",
                function (event) {

                    if (
                        event.key !== "Enter" &&
                        event.key !== " "
                    ) {
                        return;
                    }

                    event.preventDefault();

                    sortTable(
                        table,
                        header,
                        headers
                    );

                }
            );

        });

    }


    function sortTable(
        table,
        activeHeader,
        headers
    ) {

        const tbody =
            table.tBodies[0];

        if (!tbody) {
            return;
        }


        /*
         * IMPORTANT:
         *
         * Read current direction BEFORE resetting
         * the other columns.
         */
        const currentDirection =
            activeHeader.dataset.sortDirection
            || "none";


        /*
         * Toggle:
         *
         * none -> asc
         * asc  -> desc
         * desc -> asc
         */
        const nextDirection =
            currentDirection === "asc"
                ? "desc"
                : "asc";


        /*
         * Reset all OTHER headers only.
         */
        headers.forEach(function (header) {

            if (header === activeHeader) {
                return;
            }

            header.dataset.sortDirection =
                "none";

            header.setAttribute(
                "aria-sort",
                "none"
            );

            updateSortIcon(
                header,
                "none"
            );

        });


        /*
         * Update clicked header.
         */
        activeHeader.dataset.sortDirection =
            nextDirection;

        activeHeader.setAttribute(
            "aria-sort",
            nextDirection === "asc"
                ? "ascending"
                : "descending"
        );

        updateSortIcon(
            activeHeader,
            nextDirection
        );


        /*
         * Which column?
         */
        const columnIndex =
            Array.from(
                activeHeader.parentElement.children
            )
                .indexOf(activeHeader);


        const sortType =
            activeHeader.dataset.sort
            || "text";


        /*
         * Sort rows.
         */
        const rows =
            Array.from(
                tbody.rows
            );


        rows.sort(function (rowA, rowB) {

            const cellA =
                rowA.cells[columnIndex];

            const cellB =
                rowB.cells[columnIndex];


            const valueA =
                getCellValue(cellA);

            const valueB =
                getCellValue(cellB);


            let comparison =
                compareValues(
                    valueA,
                    valueB,
                    sortType
                );


            /*
             * Reverse for descending.
             */
            if (nextDirection === "desc") {
                comparison *= -1;
            }


            return comparison;

        });


        /*
         * Physically reorder existing rows.
         */
        rows.forEach(function (row) {
            tbody.appendChild(row);
        });

    }


    function getCellValue(cell) {

        if (!cell) {
            return "";
        }


        /*
         * Optional explicit value:
         *
         * <td data-sort-value="2026-09-01">
         */
        if (
            cell.dataset.sortValue !== undefined
        ) {
            return cell.dataset.sortValue.trim();
        }


        return cell.textContent
            .replace(/\s+/g, " ")
            .trim();

    }


    function compareValues(
        valueA,
        valueB,
        type
    ) {

        const a =
            valueA.trim();

        const b =
            valueB.trim();


        /*
         * Blank values always last
         */
        const aEmpty =
            !a ||
            a === "—";

        const bEmpty =
            !b ||
            b === "—";


        if (aEmpty && bEmpty) {
            return 0;
        }

        if (aEmpty) {
            return 1;
        }

        if (bEmpty) {
            return -1;
        }


        /*
         * Numbers
         */
        if (type === "number") {

            const numberA =
                Number(a);

            const numberB =
                Number(b);

            if (
                !Number.isNaN(numberA) &&
                !Number.isNaN(numberB)
            ) {
                return numberA - numberB;
            }

        }


        /*
         * Dates
         */
        if (type === "date") {

            const dateA =
                Date.parse(a);

            const dateB =
                Date.parse(b);

            if (
                !Number.isNaN(dateA) &&
                !Number.isNaN(dateB)
            ) {
                return dateA - dateB;
            }

        }


        /*
         * Text
         */
        return a.localeCompare(
            b,
            undefined,
            {
                sensitivity: "base",
                numeric: true
            }
        );

    }


    function updateSortIcon(
        header,
        direction
    ) {

        const icon =
            header.querySelector(
                ".sort-icon"
            );

        if (!icon) {
            return;
        }


        if (direction === "asc") {

            icon.className =
                "bi bi-chevron-up sort-icon";

            return;
        }


        if (direction === "desc") {

            icon.className =
                "bi bi-chevron-down sort-icon";

            return;
        }


        icon.className =
            "bi bi-chevron-expand sort-icon";

    }

});