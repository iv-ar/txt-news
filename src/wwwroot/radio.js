const app = document.getElementById("app");
let series;

async function get_series() {
    app.innerHTML = "<i>Laster...</i>";
    const response = await fetch("/radio-series");
    if (response.ok) {
        series = await response.json();
        return series;
    }
    app.innerHTML = "<strong style='color:red;'>Kunne ikke hente data, prøv igjen snart!</strong>";
    return null;
}

function search(query) {
    if (!query) {
        document.querySelectorAll("#series li").forEach(el => {
            el.style.display = "list-item";
            el.innerHTML = el.innerHTML
                .replaceAll("<b>", "")
                .replaceAll("</b>", "");
        });
        return;
    }
    console.log("Searching for " + query);
    const results = fuzzysort.go(query, series, {key: "name"});
    console.log("Found " + results.length + " results");
    for (const seriesEl of document.querySelectorAll("#series li")) {
        const matchIndex = results.findIndex(p => p.obj.id === parseInt(seriesEl.dataset.id));
        const isMatch = matchIndex !== -1;
        seriesEl.style.display = isMatch ? "list-item" : "none";
        if (isMatch) {
            seriesEl.querySelector("summary").innerHTML = fuzzysort.highlight(results[matchIndex], "<b>", "</b>");
        }
    }
}

function expand_series(listItem) {
    console.log("Toggling " + listItem.dataset.id);
}

function build_frontpage(series) {
    const listEl = document.createElement("ul");
    listEl.id = "series";
    listEl.style.listStyle = "none";
    listEl.style.maxHeight = "70vh";
    listEl.style.overflow = "auto";
    listEl.style.padding = "0";
    for (const serie of series) {
        const listItemEl = document.createElement("li");
        const listItemDetailsEl = document.createElement("details");
        const listItemDetailsSummaryEl = document.createElement("summary");
        listItemEl.dataset.id = serie.id;
        listItemDetailsSummaryEl.innerHTML = "<span>" + serie.name + "</span>";
        listItemDetailsEl.appendChild(listItemDetailsSummaryEl);
        listItemDetailsEl.ontoggle = (event) => {
            expand_series(event.target.parentNode);
        };
        listItemDetailsEl.style.cursor = "pointer";
        listItemEl.appendChild(listItemDetailsEl);
        listEl.appendChild(listItemEl);
    }

    const searchInputEl = document.createElement("input");
    searchInputEl.id = "series-searcher";
    searchInputEl.placeholder = "Søk her";
    searchInputEl.oninput = (event) => {
        search(event.target.value);
    };
    app.innerHTML = "";
    app.appendChild(searchInputEl);
    app.appendChild(listEl);
}

document.addEventListener("DOMContentLoaded", () => {
    get_series().then(build_frontpage);
})