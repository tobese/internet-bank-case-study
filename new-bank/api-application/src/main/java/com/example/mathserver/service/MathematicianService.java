package com.example.mathserver.service;

import com.example.mathserver.model.Mathematician;
import com.example.mathserver.repository.MathematicianRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.time.LocalDate;
import java.util.List;
import java.util.Optional;

@Service
public class MathematicianService {
    
    @Autowired
    private MathematicianRepository repository;
    
    public Optional<Mathematician> getRandom() {
        return repository.findRandom();
    }

    public Optional<Mathematician> getDaily() {
        List<Mathematician> all = repository.findAllByOrderByIdAsc();
        if (all.isEmpty()) return Optional.empty();
        int index = (int) (LocalDate.now().toEpochDay() % all.size());
        return Optional.of(all.get(index));
    }
    
    public Optional<Mathematician> getByName(String name) {
        return repository.findByName(name);
    }
    
    public List<Mathematician> getAll() {
        return repository.findAll();
    }
    
    public Mathematician save(Mathematician mathematician) {
        return repository.save(mathematician);
    }
    
    public Optional<Mathematician> getById(Long id) {
        return repository.findById(id);
    }
    
    public void delete(Long id) {
        repository.deleteById(id);
    }
}
