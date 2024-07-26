"use strict";
function highlight(option) {
    if (highlighted != null)
        highlighted.style.borderBottom = ""
    option.style.borderBottom = "3px solid #ccc"
    highlighted = option
}

function appendAccountToList(account) {
    let accountEntry = document.getElementById("accountTemplate")
    const content = accountEntry.content.cloneNode(true)
    content.querySelector("label").textContent = account.name
    document.getElementById("friendsList").appendChild(content)
}

let highlighted = null
let mineOption = document.getElementById("mineOption")
let allOption = document.getElementById("allOption")
highlight(mineOption)

mineOption.addEventListener('click', () => {
    highlight(mineOption)
})
allOption.addEventListener('click', () => {
    highlight(allOption)
})

var connection = new signalR.HubConnectionBuilder().withUrl("/friends").build();

connection.on("GetFriends", (accounts) => {
    for (let i = 0; i < accounts.length; i++){
        appendAccountToList(accounts[i])
    }
})

connection.start().then(() => {
    connection.invoke("GetFriends")
})
