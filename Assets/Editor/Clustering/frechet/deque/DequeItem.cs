
/**
 *
 * @author Wouter Meulemans (w.meulemans@tue.nl)
 */
public abstract class DequeItem<T> where T : DequeItem<T> {
    
    private T previous, next;
    private Deque<T> deque;

    public DequeItem() {
        previous = default(T);
        next = default(T);
        deque = default(Deque<T>);
    }

    public Deque<T> getDeque() {
        return deque;
    }

    public void setDeque(Deque<T> deque) {
        this.deque = deque;
    }

    public T getNext() {
        return next;
    }

    public void setNext(T next) {
        this.next = next;
    }

    public T getPrevious() {
        return previous;
    }

    public void setPrevious(T prev) {
        this.previous = prev;
    }
}
