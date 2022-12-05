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
        document.querySelectorAll("#series li").forEach(el => el.style.display = "list-item");
        return;
    }
    console.log("Searching for " + query);
    const results = fuzzysort.go(query, series, {key: "name"});
    console.log("Found " + results.length + " results");
    const ids = results.map(x => x.obj.id);
    for (const seriesEl of document.querySelectorAll("#series li")) {
        seriesEl.style.display = ids.includes(parseInt(seriesEl.dataset.id)) ? "list-item" : "none";
    }
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
        listItemDetailsSummaryEl.innerText = serie.name;
        listItemDetailsEl.appendChild(listItemDetailsSummaryEl);
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