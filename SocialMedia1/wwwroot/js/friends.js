"use strict";
function highlight(option) {
    if (highlighted != null)
        highlighted.style.borderBottom = ""
    option.style.borderBottom = "3px solid #ccc"
    highlighted = option
}

function removeBlockWithButton(button) {
    button.parentNode.parentNode.remove()
}

function setAccountsList(accounts, setContent) {
    friendsList.innerHTML = ""
    let accountEntry = document.getElementById("accountTemplate")
    for (let i = 0; i < accounts.length; i++) {
        const content = accountEntry.content.cloneNode(true)
        setCommonContent(content, accounts[i])
        setContent(content, accounts[i])
        friendsList.appendChild(content)
    }
}

function setBlockForAllOption(content, account) {
    let button = content.querySelector("input")
    button.value = "add"
    if (account.canRequest) {
        button.addEventListener("click", () => {
            connection.invoke("AddFriend", account.id)
            button.style.display = "none"
        })
    } else {
        button.style.display = "none"
    }
}

function setBlockForMineOption(content, account) {
    let chatButton = content.querySelectorAll("input")[1]
    chatButton.style.display = "inline-block"
    chatButton.addEventListener("click", () => {
        connection.invoke("GoToChat", account.id)
    })
    let button = content.querySelector("input")
    button.value = "delete"
    button.addEventListener("click", () => {
        connection.invoke("DeleteFriend", account.id)
        removeBlockWithButton(button)
    })
}

function setBlockForRequestsToMe(content, account) {
    let buttons = content.querySelectorAll("input")
    buttons[0].value = "aprove"
    buttons[0].addEventListener("click", () => {
        connection.invoke("AddFriend", account.id)
        removeBlockWithButton(buttons[0])
    })
    buttons[1].style.display = "inline-block"
    buttons[1].addEventListener("click", () => {
        connection.invoke("RejectRequest", account.id)
        removeBlockWithButton(buttons[1])
    })
    buttons[1].value = "reject"
}

function setBlockForRequestsFromMe(content, account) {
    let button = content.querySelector("input")
    button.value = "delete"
    button.addEventListener("click", () => {
        connection.invoke("RecallRequest", account.id)
        removeBlockWithButton(button)
    })
}

function setCommonContent(content, account) {
    let nameLabel = content.querySelector("label")
    nameLabel.textContent = account.name + " " + account.lastName
    nameLabel.addEventListener("click", () => {
        let accUrl = "/Home/Account?id=" + account.id
        window.location.href = accUrl
    })
}

let highlighted = null
const mineOption = document.getElementById("mineOption")
const allOption = document.getElementById("allOption")
const inRequestsOption = document.getElementById("inRequestsOption")
const outRequestsOption = document.getElementById("outRequestsOption")
const friendsList = document.getElementById("friendsList")
const searchBar = document.getElementById("searchBar")

const optionsActionDict = new Map([
    [mineOption, "GetMineAccounts"],
    [allOption, "GetAllAccounts"],
    [inRequestsOption, "GetRequestsToMe"],
    [outRequestsOption, "GetRequestsFromMe"]
])

for (let optionAction of optionsActionDict) {
    let option = optionAction[0]
    let serverAction = optionAction[1]
    option.addEventListener('click', () => {
        highlight(option)
        console.log("invoking " + serverAction)
        connection.invoke(serverAction, searchBar.value)
    })
}

searchBar.addEventListener('input', (event) => {
    let serverAction = optionsActionDict.get(highlighted)
    connection.invoke(serverAction, event.target.value)
})

var connection = new signalR.HubConnectionBuilder().withUrl("/friends").build();

connection.on("GetAllAccounts", (accounts) => {
    setAccountsList(accounts, setBlockForAllOption)
})

connection.on("GetMineAccounts", (accounts) => {
    setAccountsList(accounts, setBlockForMineOption)
})

connection.on("GetRequestsToMe", (accounts) => {
    setAccountsList(accounts, setBlockForRequestsToMe)
})

connection.on("GetRequestsFromMe", (accounts) => {
    setAccountsList(accounts, setBlockForRequestsFromMe)
})

connection.on("GoToChat", (chatId) => {
    let chatUrl = "/Home/Chats?id=" + chatId
    window.location.href = chatUrl
})

connection.start().then(() => {
    connection.invoke("GetAllAccounts", "")
    highlight(allOption)
})
