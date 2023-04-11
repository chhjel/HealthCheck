Array.prototype.joinForSentence = function<T> (this : Array<T>, delimiter: string = ", ", word: string = "and") {
  if (this == null || this.length <= 1) {
    return this[0];
  }
  else if (this.length == 2)
  {
    return `${this[0]} ${word} ${this[1]}`
  }

  let count = this.length;
  var firstParts = this.slice(0, count - 1);
  var lastPart = this[count-1];
  return `${firstParts.join(delimiter)}${word}${lastPart}`;
};
