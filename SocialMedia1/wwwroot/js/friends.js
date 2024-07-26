"use strict";
function highlight(option) {
    if (highlighted != null)
        highlighted.style.borderBottom = ""
    option.style.borderBottom = "3px solid #ccc"
    highlighted = option
}

function appendAccountToList(account, buttonActionSetter) {
    let accountEntry = document.getElementById("accountTemplate")
    const content = accountEntry.content.cloneNode(true)
    content.querySelector("label").textContent = account.account.name
    let button = content.querySelector("input")
    buttonActionSetter(button, account)
    friendsList.appendChild(content)
}

function setAccountsList(accounts, setContent) {
    friendsList.innerHTML = ""
    let accountEntry = document.getElementById("accountTemplate")
    for (let i = 0; i < accounts.length; i++) {
        const content = accountEntry.content.cloneNode(true)
        setContent(content, accounts[i])
    }
}

function setBlockForAllOption(content, account) {
    content.querySelector("label").textContent = account.account.name
    let button = content.querySelector("input")
    button.value = "add"
    if (!account.isFriend) {
        button.addEventListener("click", () => {
            connection.invoke("AddFriend", account.account.id)
            button.disabled = true
        })
    } else {
        button.disabled = true
    }
    friendsList.appendChild(content)
}

function setBlockForMineOption(content, account) {
    content.querySelector("label").textContent = account.name
    let button = content.querySelector("input")
    button.value = "delete"
    friendsList.appendChild(content)
}

let highlighted = null
let mineOption = document.getElementById("mineOption")
let allOption = document.getElementById("allOption")
let friendsList = document.getElementById("friendsList")

mineOption.addEventListener('click', () => {
    highlight(mineOption)
    connection.invoke("GetMineAccounts")
})
allOption.addEventListener('click', () => {
    highlight(allOption)
    connection.invoke("GetAllAccounts")
})

var connection = new signalR.HubConnectionBuilder().withUrl("/friends").build();

connection.on("GetAllAccounts", (accounts) => {
    setAccountsList(accounts, setBlockForAllOption)
})

connection.on("GetMineAccounts", (accounts) => {
    setAccountsList(accounts, setBlockForMineOption)
})

connection.start().then(() => {
    connection.invoke("GetAllAccounts")
    highlight(allOption)
})
