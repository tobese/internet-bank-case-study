package com.example.mathserver.repository;

import com.example.mathserver.model.Mathematician;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

@Repository
public interface MathematicianRepository extends JpaRepository<Mathematician, Long> {
    
    @Query(value = "SELECT * FROM mathematicians ORDER BY RANDOM() LIMIT 1", nativeQuery = true)
    Optional<Mathematician> findRandom();
    
    Optional<Mathematician> findByName(String name);

    List<Mathematician> findAllByOrderByIdAsc();
}
