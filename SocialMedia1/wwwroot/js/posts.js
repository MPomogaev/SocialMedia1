import { DateEntryHandler, dateToString } from "./dateHandler.js"

const connection = new signalR.HubConnectionBuilder().withUrl("/posts").build();

const postsBlock = document.getElementById("postsBlock")

const postTemplate = document.getElementById("postTemplate")
const postDateTemplate = document.getElementById("postDateTemplate")

let dateHandler = new DateEntryHandler(postDateTemplate, postsBlock)

function setPost(post) {
    let postEntry = postTemplate.content.cloneNode(true)
    postEntry.querySelector(".creatorsName").textContent =
        post.account.name + " "
        + post.account.lastName
    postEntry.querySelector(".creatorsAccountImage").src =
        "data:image/png;base64," + post.account.profilePhoto
    postEntry.querySelector(".creatorsAccount")
        .addEventListener("click", () => {
            let url = "/Home/Account?id=" + post.account.id
            window.location.href = url
        })
    postEntry.querySelector(".postTitle").textContent = post.post.title
    postEntry.querySelector(".postText").textContent = post.post.text
    let postDate = new Date(post.post.createdDate)
    postEntry.querySelector(".postTime").textContent = dateToString(postDate)
    dateHandler.setDate(postDate)
    postsBlock.appendChild(postEntry)
}

connection.start().then(() => {
    connection.invoke("GetPosts")
})

connection.on("GetPosts", (posts) => {
    console.log("receiving posts " + posts.length)
    for (let post of posts) {
        setPost(post)
    }
})