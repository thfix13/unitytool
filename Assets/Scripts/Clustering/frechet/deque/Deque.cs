
/**
 * A simple doubly ended queue.
 * 
 * @author Wouter Meulemans (w.meulemans@tue.nl)
 */
public class Deque<T> where T : DequeItem<T> {

    private T head, tail;
    private int size;

    public Deque() {
        head = default(T);
        tail = default(T);
        size = 0;
    }

    public int getSize() {
        return size;
    }

    public T getFirst() {
        return head;
    }

    public T getLast() {
        return tail;
    }

    public void addFirst(T item) {
//        assert item.getDeque() == null;

        item.setDeque(this);
        if (size == 0) {
            head = item;
            tail = item;
        } else {
            head.setPrevious(item);
            item.setNext(head);
            head = item;
        }
        size++;
    }

    public void addLast(T item) {
//        assert item.getDeque() == null;

        item.setDeque(this);
        if (size == 0) {
            head = item;
            tail = item;
        } else {
            tail.setNext(item);
            item.setPrevious(tail);
            tail = item;
        }
        size++;
    }

    public void clear() {
        while (size > 0) {
            removeFirst();
        }
    }

    public T removeFirst() {
 //       assert size > 0;

        T removed = head;

        head.setDeque(null);
        if (size == 1) {
            head = null;
            tail = null;
        } else {
            T second = head.getNext();
            second.setPrevious(null);
            head.setNext(null);
            head = second;
        }
        size--;

        return removed;
    }

    public T removeLast() {
  //      assert size > 0;

        T removed = tail;

        tail.setDeque(null);
        if (size == 1) {
            head = null;
            tail = null;
        } else {
            T beforeLast = tail.getPrevious();
            beforeLast.setNext(null);
            tail.setPrevious(null);
            tail = beforeLast;
        }
        size--;

        return removed;
    }

    public void remove(T elt) {
        if (elt == head) {
            removeFirst();
        } else if (elt == tail) {
            removeLast();
        } else {

            elt.setDeque(null);

            elt.getPrevious().setNext(elt.getNext());
            elt.getNext().setPrevious(elt.getPrevious());

            elt.setNext(null);
            elt.setPrevious(null);

            size--;
        }
    }
}
