"use strict";
function highlight(option) {
    if (highlighted != null)
        highlighted.style.borderBottom = ""
    option.style.borderBottom = "3px solid #ccc"
    highlighted = option
}


function setAccountsList(accounts, setContent) {
    friendsList.innerHTML = ""
    let accountEntry = document.getElementById("accountTemplate")
    const searchString = searchBar.value
    for (let i = 0; i < accounts.length; i++) {
        const content = accountEntry.content.cloneNode(true)
        setContent(content, accounts[i], searchString)
    }
}

function setBlockForAllOption(content, account) {
    setCommonContent(content, account.account)
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
    setCommonContent(content, account)
    let button = content.querySelector("input")
    button.value = "delete"
    button.addEventListener("click", () => {
        connection.invoke("DeleteFriend", account.id)
        button.parentNode.remove()
    })
    friendsList.appendChild(content)
}

function setCommonContent(content, account) {
    let nameLabel = content.querySelector("label")
    nameLabel.textContent = account.name + " " + account.lastName
    nameLabel.addEventListener("click", () => {
        let accUrl = "/Home/Account?id=" + account.id
        window.location.href = accUrl
    })
    let button = content.querySelectorAll("input")[1]
    button.addEventListener("click", () => {
        connection.invoke("GoToChat", account.id)
    })
}

let highlighted = null
const mineOption = document.getElementById("mineOption")
const allOption = document.getElementById("allOption")
const friendsList = document.getElementById("friendsList")
const searchBar = document.getElementById("searchBar")

mineOption.addEventListener('click', () => {
    highlight(mineOption)
    connection.invoke("GetMineAccounts", searchBar.value)
})
allOption.addEventListener('click', () => {
    highlight(allOption)
    connection.invoke("GetAllAccounts", searchBar.value)
})
searchBar.addEventListener('input', (event) => {
    if (highlighted == mineOption) {
        connection.invoke("GetMineAccounts", event.target.value)
    } else {
        connection.invoke("GetAllAccounts", event.target.value)
    }
})

var connection = new signalR.HubConnectionBuilder().withUrl("/friends").build();

connection.on("GetAllAccounts", (accounts) => {
    setAccountsList(accounts, setBlockForAllOption)
})

connection.on("GetMineAccounts", (accounts) => {
    setAccountsList(accounts, setBlockForMineOption)
})

connection.on("GoToChat", (chatId) => {
    let chatUrl = "/Home/Chats?id=" + chatId
    window.location.href = chatUrl
})

connection.start().then(() => {
    connection.invoke("GetAllAccounts", "")
    highlight(allOption)
})
