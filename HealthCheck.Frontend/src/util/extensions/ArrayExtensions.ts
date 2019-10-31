Array.prototype.joinForSentence = function<T> (this : Array<T>, delimiter: string = ", ", word: string = "and") {
  if (this == null || this.length <= 1) {
    return this[0];
  }

  let count = this.length;
  let combined = '';
  for (let i=0; i<count - 1; i++)
  {
    combined += `${this[i]}${delimiter}`;
  }
  
  combined += ` ${word} ${this[count-1]}`;
  return combined;
};
