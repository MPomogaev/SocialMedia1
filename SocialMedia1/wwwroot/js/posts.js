const connection = new signalR.HubConnectionBuilder().withUrl("/posts").build();

const postsBlock = document.getElementById("postsBlock")

const postTemplate = document.getElementById("postTemplate")
const postDateTemplate = document.getElementById("postDateTemplate")

let thisYear = (new Date()).getFullYear()
let lastPostDate = new Date()

function compareDates(firstDate, secondDate) {
    console.log(firstDate)
    console.log(secondDate)
    return firstDate.getDate() == secondDate.getDate()
        && firstDate.getFullYear() == secondDate.getFullYear()
}

function appendDateToPostList(postDate) {
    if (!compareDates(lastPostDate, postDate)) {
        lastPostDate = postDate
        let dateEntry = postDateTemplate.content.cloneNode(true)
        let dateStr = lastPostDate.getDate() + " "
            + lastPostDate.toLocaleString('default', { month: 'long' })
        let year = postDate.getFullYear()
        if (thisYear != year) {
            dateStr += " " + year
        }
        dateEntry.querySelector("label").textContent = dateStr
        postsBlock.appendChild(dateEntry)
    }
}

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
    postEntry.querySelector(".postTime").textContent =
        postDate.getHours() + ":"
        + postDate.getMinutes() + ":"
        + postDate.getSeconds()
    appendDateToPostList(postDate)
    postsBlock.appendChild(postEntry)
    console.log(post.post.text)
}

connection.start().then(() => {
    console.log("send getting posts")
    connection.invoke("GetPosts")
})

connection.on("GetPosts", (posts) => {
    console.log("receive getting posts " + posts.length)
    for (post of posts) {
        setPost(post)
    }
})