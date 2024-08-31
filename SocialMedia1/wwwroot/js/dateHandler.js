export class DateEntryHandler {

    constructor(dateTemplate, contentList) {
        this.lastDate = new Date()
        this.thisYear = this.lastDate.getFullYear()
        this.dateTemplate = dateTemplate
        this.contentList = contentList
    }

    compareDates(firstDate, secondDate) {
        return firstDate.getDate() == secondDate.getDate()
            && firstDate.getFullYear() == secondDate.getFullYear()
    }

    getDateString(date) {
        let dateStr = this.lastDate.getDate() + " "
            + this.lastDate.toLocaleString('default', { month: 'long' })
        let year = date.getFullYear()
        if (this.thisYear != year) {
            dateStr += " " + year
        }
        return dateStr
    }

    setDate(date) {
        if (this.compareDates(this.lastDate, date)) {
            return
        }
        this.lastDate = date
        let dateEntry = this.dateTemplate.content.cloneNode(true)
        let dateStr = this.getDateString(date)
        dateEntry.querySelector("label").textContent = dateStr
        this.contentList.appendChild(dateEntry)
    }
}

export function dateToString(date) {
    return date.getHours() + ":"
        + date.getMinutes() + ":"
        + date.getSeconds()
}