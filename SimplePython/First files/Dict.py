import sys

def makePaymentDict():
    payments = { 1:('Petrov', 2000), 2:('Ivanov', 3000) }
    return payments

def dictFormating():
    hash = {}
    hash['word'] = 'листов'
    hash['count'] = 42
    s = 'I want %(count)d copies of %(word)s' % hash  # %d for int, %s for string
    print(s)


if __name__ == '__main__':
 	dictFormating()
